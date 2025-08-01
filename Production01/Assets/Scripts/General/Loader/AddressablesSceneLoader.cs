using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class AddressablesSceneLoader
{
    private ILogger _Logger;
    private readonly object _lock = new();
    public AddressablesSceneLoader(ILogger logger)
    {
        _Logger = logger;
    }

    /// <summary>
    /// 非同期のシーンロード
    /// AdditiveがDefault
    /// </summary>
    /// <param name="sceneKey"></param>
    /// <returns></returns>
    public async Task<SceneLoadResult> LoadSceneAsync(string sceneKey, CancellationToken token = default)
    {
        try
        {
            //非アクティブにしておく
            var op = Addressables.LoadSceneAsync(sceneKey, LoadSceneMode.Additive, false);
            while (!op.IsDone)
            {
                //キャンセルが出たら例外発生
                token.ThrowIfCancellationRequested();
                await Task.Yield();
            }
            //問題なくロードできているなら
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                _Logger.Log($"Loaded scene: {sceneKey}");
                return new SceneLoadResult(op.Result);
            }

            return new SceneLoadResult(AssetLoadErrorType.NotFound, $"Failed during load: {sceneKey}");
        }
        catch (OperationCanceledException)
        {
            _Logger.LogWarning($"Scene load canceled: {sceneKey}");
            return new SceneLoadResult(AssetLoadErrorType.Canceled, "Canceled");
        }
        catch (Exception e)
        {
            return new SceneLoadResult(AssetLoadErrorType.Exception, $"Exception during load: {e.Message}");
        }
    }

    /// <summary>
    /// ロードするシーンの読み込み進捗を渡す
    /// </summary>
    /// <param name="sceneKey"></param>
    /// <param name="progress"></param>
    /// <returns></returns>
    public async Task<SceneLoadResult> LoadSceneWithProgressAsync(string sceneKey, IProgressReporter progress = null, CancellationToken token = default)
    {
        try
        {
            var op = Addressables.LoadSceneAsync(sceneKey, LoadSceneMode.Additive, false);

            while (!op.IsDone)
            {
                //キャンセル例外
                token.ThrowIfCancellationRequested();
                progress?.Report(op.PercentComplete);
                await Task.Yield();
            }

            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                _Logger.Log($"Loaded scene: {sceneKey}");
                return new SceneLoadResult(op.Result);
            }

            return new SceneLoadResult(AssetLoadErrorType.NotFound, $"Failed to load scene: {sceneKey}");
        }
        catch (OperationCanceledException)
        {
            _Logger.LogWarning($"Scene load canceled: {sceneKey}");
            return new SceneLoadResult(AssetLoadErrorType.Canceled, "Canceled");
        }
        catch (Exception e)
        {
            return new SceneLoadResult(AssetLoadErrorType.Exception, $"Exception during scene load: {e.Message}");
        }
    }

    /// <summary>
    /// 複数のシーンをロードする
    /// </summary>
    /// <param name="sceneKeys"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<SceneLoadResult[]> LoadMultipleScenesAsync(string[] sceneKeys, CancellationToken token = default)
    {
        SceneLoadResult[] loadResults = new SceneLoadResult[sceneKeys.Length];
        List<Task<SceneLoadResult>> loadTasks = new List<Task<SceneLoadResult>>();

        //foreachを使うことでクロージャー機能が活きてopやkeyにアクセスできる
        foreach (var key in sceneKeys)
        {
            var op = Addressables.LoadSceneAsync(key, LoadSceneMode.Additive, false);

            //ロード完了待ち
            async Task<SceneLoadResult> LoadSceneAsync()
            {
                try
                {
                    while (!op.IsDone)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.Yield();
                    }

                    if (op.Status == AsyncOperationStatus.Succeeded)
                    {
                        _Logger.Log($"Loaded scene: {key}");
                        return new SceneLoadResult(op.Result);
                    }

                    return new SceneLoadResult(AssetLoadErrorType.NotFound, $"Failed during load: {key}");
                }
                catch (OperationCanceledException)
                {
                    _Logger.LogWarning($"Scene load canceled: {key}");
                    return new SceneLoadResult(AssetLoadErrorType.Canceled, "Canceled");
                }
                catch (Exception e)
                {
                    _Logger.LogWarning($"Exception during load: {e.Message}");
                    return new SceneLoadResult(AssetLoadErrorType.Exception, $"Exception: {e.Message}");
                }
            }

            loadTasks.Add(LoadSceneAsync());
        }

        return await Task.WhenAll(loadTasks);
    }

    /// <summary>
    /// 複数シーン読み込み+読み込み進捗も返す
    /// </summary>
    /// <param name="sceneKeys"></param>
    /// <param name="progress"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<SceneLoadResult[]> LoadMultipleScenesAsync(string[] sceneKeys, IProgressReporter progress, CancellationToken token = default)
    {
        SceneLoadResult[] loadResults = new SceneLoadResult[sceneKeys.Length];
        List<Task<SceneLoadResult>> loadTasks = new List<Task<SceneLoadResult>>();
        int completed = 0;
        int total = sceneKeys.Length;
        //foreachを使うことでクロージャー機能が活きてopやkeyにアクセスできる
        foreach (var key in sceneKeys)
        {
            var op = Addressables.LoadSceneAsync(key, LoadSceneMode.Additive, false);

            //ロード完了待ち
            async Task<SceneLoadResult> LoadSceneAsync()
            {
                try
                {
                    while (!op.IsDone)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.Yield();
                    }

                    if (op.Status == AsyncOperationStatus.Succeeded)
                    {
                        _Logger.Log($"Loaded scene: {key}");
                        //トータルに対して完了した数で進捗を送る
                        ++completed;
                        progress?.Report((float)(completed / total));
                        return new SceneLoadResult(op.Result);
                    }
                    return new SceneLoadResult(AssetLoadErrorType.NotFound, $"Failed during load: {key}");
                }
                catch (OperationCanceledException)
                {
                    _Logger.LogWarning($"Scene load canceled: {key}");
                    return new SceneLoadResult(AssetLoadErrorType.Canceled, "Canceled");
                }
                catch (Exception e)
                {
                    _Logger.LogWarning($"Exception during load: {e.Message}");
                    return new SceneLoadResult(AssetLoadErrorType.Exception, $"Exception: {e.Message}");
                }
                finally
                {
                    // 成功でも失敗でもキャンセルでも進捗は更新する
                    Interlocked.Increment(ref completed);
                    progress?.Report((float)(completed / total));
                }
            }

            loadTasks.Add(LoadSceneAsync());
        }

        return await Task.WhenAll(loadTasks);
    }

    /// <summary>
    /// 読み込んだシーンのAwaitやStartを完了させる
    /// </summary>
    /// <param name="scene"></param>
    public async Task SetActiveSceneInstance(SceneInstance sceneInstance)
    {
        //これを呼ぶとそうなるらしい
        await sceneInstance.ActivateAsync();
        _Logger.Log($"Complete Active SceneInstance.Scene : {sceneInstance.Scene.name}");
    }

    /// <summary>
    /// アクティブにするシーンをセットする
    /// </summary>
    /// <param name="scene"></param>
    public void SetActiveScene(Scene scene)
    {
        lock (_lock)
        {
            SceneManager.SetActiveScene(scene);
            _Logger.Log($"Active scene: {scene.name}");
        }
    }

    //ここでしか使わない
    private readonly HashSet<string> _UnloadingScenes = new();
    /// <summary>
    /// シーンのアンロード
    /// </summary>
    /// <param name="scene"></param>
    public async Task UnloadSceneAsync(Scene scene)
    {
        string name = scene.name;

        // Unload中なら即return
        lock (_lock)
        {
            if (_UnloadingScenes.Contains(name))
            {
                _Logger.LogWarning($"Scene {name} is already unloading.");
                return;
            }
            _UnloadingScenes.Add(name);
        }

        try
        {
            //sceneごとにopが作られる
            var op = SceneManager.UnloadSceneAsync(scene);
            if (op == null)
            {
                _Logger.LogWarning($"Scene {name} is not valid or already unloaded.");
                return;
            }

            while (!op.isDone)
            {
                await Task.Yield();
            }

            _Logger.Log($"Unload complete: {name}");
        }
        finally
        {
            lock (_lock)
            {
                _UnloadingScenes.Remove(name);
            }
        }
    }

    /// <summary>
    /// Sceneを読み込んでいるか
    /// </summary>
    /// <param name="sceneName"></param>
    /// <returns></returns>
    public bool IsSceneLoaded(string sceneName)
    {
        return SceneManager.GetSceneByName(sceneName).isLoaded;
    }

    /// <summary>
    /// 欲しいシーンをもらう
    /// </summary>
    /// <param name="sceneName">シーンの名前</param>
    /// <returns>Sceneを返す</returns>
    public Scene GetSceneByName(string sceneName)
    {
        _Logger.Log($"want to scene: {sceneName}");
        return SceneManager.GetSceneByName(sceneName);
    }

    /// <summary>
    /// アクティブになってるシーンをもらう
    /// </summary>
    /// <returns></returns>
    public Scene GetActiveScene()
    {
        _Logger.Log($"Want to Active scene");
        return SceneManager.GetActiveScene();
    }
}

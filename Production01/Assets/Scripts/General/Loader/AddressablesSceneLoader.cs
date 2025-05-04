using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class AddressablesSceneLoader 
{
    private ILogger _Logger;

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
    public async ValueTask<SceneLoadResult> LoadSceneAsync(string sceneKey)
    {
        try
        {
            //非アクティブにしておく
            var op = Addressables.LoadSceneAsync(sceneKey, LoadSceneMode.Additive,false);
            await op.Task;

            //問題なくロードできているなら
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                _Logger.Log($"Loaded scene: {sceneKey}");
                return new SceneLoadResult(op.Result);
            }

            return new SceneLoadResult($"Failed during load: {sceneKey}");
        }
        catch (Exception e)
        {
            return new SceneLoadResult($"Exception during load: {e.Message}");
        }
    }

    /// <summary>
    /// ロードするシーンの読み込み進捗を渡す
    /// </summary>
    /// <param name="sceneKey"></param>
    /// <param name="progress"></param>
    /// <returns></returns>
    public async ValueTask<SceneLoadResult> LoadSceneWithProgressAsync(string sceneKey, IProgressReporter progress = null)
    {
        try
        {
            var op = Addressables.LoadSceneAsync(sceneKey, LoadSceneMode.Additive,false);

            while (!op.IsDone)
            {
                progress?.Report(op.PercentComplete);
                await Task.Yield();// フレームを跨いでループ
            }

            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                _Logger.Log($"Loaded scene: {sceneKey}");
                return new SceneLoadResult(op.Result);
            }

            return new SceneLoadResult($"Failed to load scene: {sceneKey}");
        }
        catch (Exception e)
        {
            return new SceneLoadResult($"Exception during scene load: {e.Message}");
        }
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
    /// シーンの読み込みとそのシーン内のAwait/Start/Enebleを行う
    /// </summary>
    /// <param name="sceneKey"></param>
    /// <param name="progress"></param>
    /// <returns></returns>
    public async Task LoadSceneAnsSetActiveAsync(string sceneKey, IProgressReporter progress = null)
    {
        var result = await ((progress == null) ? LoadSceneAsync(sceneKey) : LoadSceneWithProgressAsync(sceneKey, progress)); 

        //ちゃんと読み込めていたら
        if(result.IsSuccess)
        {
            _Logger.Log($"Complete Load Scene : {sceneKey}");
            await SetActiveSceneInstance(result.SceneInstance);
        }
    }

    /// <summary>
    /// アクティブにするシーンをセットする
    /// </summary>
    /// <param name="scene"></param>
    public void SetActiveScene(Scene scene)
    {
        SceneManager.SetActiveScene(scene);
        _Logger.Log($"Active scene: {scene.name}");
    }

    /// <summary>
    /// シーンのアンロード
    /// </summary>
    /// <param name="scene"></param>
    public async Task UnloadSceneAsync(Scene scene)
    {
        try
        {
            var op = SceneManager.UnloadSceneAsync(scene);
            if (op == null)
            {
                _Logger.LogWarning($"Scene {scene.name} is not valid or already unloaded.");
                return;
            }

            //Taskがないからこうやって待つしかないかも
            while (!op.isDone)
            {
                await Task.Yield();
            }

            _Logger.Log($"Unload complete: {scene.name}");
        }
        catch(Exception e)
        {
            _Logger.LogWarning($"Exception during load: {e.Message}");
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

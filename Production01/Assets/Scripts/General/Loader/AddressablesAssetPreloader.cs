using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// ゲーム起動時に必要な共通リソースをプリロードする専用クラス 
/// </summary>
public class AddressablesAssetPreloader : BaseAssetLoader
{
    public AddressablesAssetPreloader(ILogger logger) : base(logger) { }

    /// <summary>
    /// アセットを事前にロードしてキャッシュに保存
    /// </summary>
    /// <typeparam name="T">ロードする型</typeparam>
    /// <param name="key">キー</param>
    public async Task<PreloadResult> PreloadAssetAsync<T>(string key, bool retryIfReleasing = true,
        CancellationToken token = default) where T : UnityEngine.Object
    {
        List<string> successKeys = new List<string>();
        List<string> failedKeys = new List<string>();

        if (retryIfReleasing)
        {
            //数回の猶予をもって解放まで待ってみる
            var success = await WaitUntilReleasedAsync(key);
            if (!success)
            {
                _Logger.LogWarning($"Asset is still being released after retries: {key}");
                 failedKeys.Add(key);
                return new PreloadResult(successKeys,failedKeys);
            }
        }
        //このタイミング解放されてないならアウト
        else if (IsReleasing(key))
        {
            _Logger.LogWarning($"Asset is currently being released: {key}");
            failedKeys.Add(key);
            return new PreloadResult(successKeys, failedKeys);
        }

        try
        {
            // キャッシュに既にあるか確認
            if (_Cache.IsCacheAvailable(key))
            {
                //触れるのは一人のみ
                successKeys.Add(key);
                return new PreloadResult(successKeys, failedKeys);
            }

            // アセットを非同期で読み込み
            var op = Addressables.LoadAssetAsync<T>(key);
            while (!op.IsDone)
            {
                //キャンセルが出たら例外を出す
                token.ThrowIfCancellationRequested();
                await Task.Yield();
            }
            //読み込み成功時
            if(op.Status == AsyncOperationStatus.Succeeded)
            {
                _Cache.AddToCache(key, op);
                //触れるのは一人のみ
                successKeys.Add(key);
            }
            else
            {
                _Logger.LogWarning($"[Preload] Failed to load {key}: {op.OperationException}");
                //触れるのは一人のみ
                failedKeys.Add(key);
            }
        }
        catch (OperationCanceledException)
        {
            failedKeys.Add(key);
        }
        catch (Exception e)
        {
            _Logger.LogError($"Exception during load: {e.Message}");
            failedKeys.Add(key);
        }

        return new PreloadResult(successKeys, failedKeys);
    }

    /// <summary>
    /// アセットを事前にロードしてキャッシュに保存
    /// </summary>
    /// <typeparam name="T">ロードする型</typeparam>
    /// <param name="key">キー</param>
    public async Task<PreloadResult> PreloadAssetWithProgressAsync<T>(string key, IProgressReporter progress = null, 
        bool retryIfReleasing = true, CancellationToken token = default) where T : UnityEngine.Object
    {
        List<string> successKeys = new List<string>();
        List<string> failedKeys = new List<string>();

        if (retryIfReleasing)
        {
            //数回の猶予をもって解放まで待ってみる
            var success = await WaitUntilReleasedAsync(key);
            if (!success)
            {
                _Logger.LogWarning($"Asset is still being released after retries: {key}");
                failedKeys.Add(key);
                return new PreloadResult(successKeys, failedKeys);

            }
        }
        //このタイミング解放されてないならアウト
        else if (IsReleasing(key))
        {
            _Logger.LogWarning($"Asset is currently being released: {key}");
            failedKeys.Add(key);
            return new PreloadResult(successKeys, failedKeys);
        }

        try
        {
            // キャッシュに既にあるか確認
            if (_Cache.IsCacheAvailable(key))
            {
                successKeys.Add(key);
                return new PreloadResult(successKeys, failedKeys);
            }

            // アセットを非同期で読み込み
            var op = Addressables.LoadAssetAsync<T>(key);
            while (!op.IsDone)
            {
                //キャンセルが出たら例外を吐くように
                token.ThrowIfCancellationRequested();
                progress?.Report(op.PercentComplete);
                await Task.Yield();
            }
            //読み込み成功時
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                progress?.Report(1.0f);
                _Cache.AddToCache(key, op);
                successKeys.Add(key);
            }
            else
            {
                _Logger.LogWarning($"[Preload] Failed to load {key}: {op.OperationException}");
                failedKeys.Add(key);
            }
        }
        catch (OperationCanceledException)
        {
            _Logger.LogWarning($"Preload canceled: {key}");
            failedKeys.Add(key);
        }
        catch (Exception e)
        {
            _Logger.LogError($"Exception during load: {e.Message}");
            failedKeys.Add(key);
        }

        return new PreloadResult(successKeys, failedKeys);
    }

    /// <summary>
    /// ラベルに紐づくアセットを個別に非同期ロードしてキャッシュする
    /// </summary>
    /// <typeparam name="T">ロード対象の型</typeparam>
    /// <param name="label">Addressableのラベル</param>
    public async Task<PreloadResult> PreloadLabelAssetsIndividuallyAsync<T>(string label, bool retryIfReleasing = true, CancellationToken token = default) where T : UnityEngine.Object
    {
        List<string> successKeys = new();
        List<string> failedKeys = new();
        object _lock = new(); // ローカルに鍵を1つだけ用意して全タスクで共有
        try
        {
            //非同期ラベル読み込み
            var locationHandle = Addressables.LoadResourceLocationsAsync(label, typeof(T));
            await locationHandle.Task;
            List<Task> loadTasks = new();

            foreach (var location in locationHandle.Result)
            {
                if (retryIfReleasing)
                {
                    //数回の猶予をもって解放まで待ってみる
                    var success = await WaitUntilReleasedAsync(location.PrimaryKey);
                    if (!success)
                    {
                        _Logger.LogWarning($"Asset is still being released after retries: {location.PrimaryKey}");
                        //触れるのは一人のみ
                        lock (_lock) failedKeys.Add(location.PrimaryKey);
                        continue;
                    }
                }
                //このタイミング解放されてないならアウト
                else if (IsReleasing(location.PrimaryKey))
                {
                    _Logger.LogWarning($"Asset is currently being released: {location.PrimaryKey}");
                    //触れるのは一人のみ
                    lock (_lock) failedKeys.Add(location.PrimaryKey);
                    continue;
                }

                // アセットを非同期で読み込み
                var op = Addressables.LoadAssetAsync<T>(location);

                //Load後に行ってほしい処理
                async Task LoadAndCache()
                {
                    try
                    {
                        while (!op.IsDone)
                        {
                            //Cancelされたら反応
                            token.ThrowIfCancellationRequested();
                            await Task.Yield();
                        }

                        if (op.Status == AsyncOperationStatus.Succeeded)
                        {
                            _Cache.AddToCache(location.PrimaryKey, op);
                            //触れるのは一人のみ
                            lock (_lock) successKeys.Add(location.PrimaryKey);
                        }
                        else
                        {
                            _Logger.LogWarning($"[Preload] Failed to load {location.PrimaryKey}: {op.OperationException}");
                            //触れるのは一人のみ
                            lock (_lock) failedKeys.Add(location.PrimaryKey);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        _Logger.LogWarning($"[Preload] Canceled during load: {location.PrimaryKey}");
                        //触れるのは一人のみ
                        lock (_lock) failedKeys.Add(location.PrimaryKey);
                    }
                    catch (Exception e)
                    {
                        _Logger.LogWarning($"[Preload] Exception while loading {location.PrimaryKey}: {e.Message}");
                        //触れるのは一人のみ
                        lock (_lock) failedKeys.Add(location.PrimaryKey);
                    }
                }

                loadTasks.Add(LoadAndCache());
            }

            //全部アセットのロードが終わるまで待機
            await Task.WhenAll(loadTasks);
        }
        catch (Exception e)
        {
            _Logger.LogError($"Exception during load: {e.Message}");
            // エラーが発生した場合、全部失敗扱いで返す
            return new PreloadResult(successKeys, failedKeys.Count > 0 ? failedKeys : new List<string> { $"[LabelLoadFailed] {label}" });
        }

        return new PreloadResult(successKeys, failedKeys);
    }

    /// <summary>
    /// ラベルに紐づくアセットを個別に非同期ロードしてキャッシュする
    /// また、読み込みの進捗を渡す
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="label"></param>
    /// <param name="progress"></param>
    /// <returns></returns>
    public async Task<PreloadResult> PreloadLabelAssetsIndividuallyWithProgressAsync<T>(string label, IProgressReporter progress = null, 
        bool retryIfReleasing = true, CancellationToken token = default) where T : UnityEngine.Object
    {
        List<string> successKeys = new();
        List<string> failedKeys = new();
        object _lock = new(); // ローカルに鍵を1つだけ用意して全タスクで共有
        try
        {
            //非同期ラベル読み込み
            var locationHandle = Addressables.LoadResourceLocationsAsync(label, typeof(T));
            await locationHandle.Task;
            List<Task> loadTasks = new();
            int total = locationHandle.Result.Count;
            int completed = 0;

            foreach (var location in locationHandle.Result)
            {
                if (retryIfReleasing)
                {
                    //数回の猶予をもって解放まで待ってみる
                    var success = await WaitUntilReleasedAsync(location.PrimaryKey);
                    if (!success)
                    {
                        _Logger.LogWarning($"Asset is still being released after retries: {location.PrimaryKey}");
                        //触れるのは一人のみ
                        lock (_lock) failedKeys.Add(location.PrimaryKey);
                        Interlocked.Decrement(ref total);
                        continue;
                    }
                }
                //このタイミング解放されてないならアウト
                else if (IsReleasing(location.PrimaryKey))
                {
                    _Logger.LogWarning($"Asset is currently being released: {location.PrimaryKey}");
                    //触れるのは一人のみ
                    lock (_lock) failedKeys.Add(location.PrimaryKey);
                    Interlocked.Decrement(ref total);
                    continue;
                }

                // アセットを非同期で読み込み
                var op = Addressables.LoadAssetAsync<T>(location);

                //Load後に行ってほしい処理
                async Task LoadAndCache()
                {
                    try
                    {
                        while (!op.IsDone)
                        {
                            token.ThrowIfCancellationRequested();
                            await Task.Yield();
                        }
                        //成功時のみキャッシュに登録
                        if (op.Status == AsyncOperationStatus.Succeeded)
                        {
                            _Cache.AddToCache(location.PrimaryKey, op);
                            //触れるのは一人のみ
                            lock (_lock) successKeys.Add(location.PrimaryKey);
                        }
                        else
                        {
                            _Logger.LogWarning($"Failed to preload {location.PrimaryKey}: {op.OperationException}");
                            //触れるのは一人のみ
                            lock (_lock) failedKeys.Add(location.PrimaryKey);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        _Logger.LogWarning($"[Preload] Canceled during load: {location.PrimaryKey}");
                        //触れるのは一人のみ
                        lock (_lock) failedKeys.Add(location.PrimaryKey);
                    }
                    catch (Exception e)
                    {
                        _Logger.LogWarning($"[Preload] Exception while loading {location.PrimaryKey}: {e.Message}");
                        //触れるのは一人のみ
                        lock (_lock) failedKeys.Add(location.PrimaryKey);
                    }
                    finally
                    {
                        //トータルに対して完了した数で進捗を送る
                        Interlocked.Increment(ref completed);//おさわりは一人ずつ
                        progress?.Report((float)completed / total);
                    }
                }
                loadTasks.Add(LoadAndCache());
            }

            //全部アセットのロードが終わるまで待機
            await Task.WhenAll(loadTasks);
        }
        catch (Exception e)
        {
            _Logger.LogError($"Exception during load: {e.Message}");
            // エラーが発生した場合、全部失敗扱いで返す
            return new PreloadResult(successKeys, failedKeys.Count > 0 ? failedKeys : new List<string> { $"[LabelLoadFailed] {label}" });
        }

        return new PreloadResult(successKeys, failedKeys);
    }

    /// <summary>
    /// プリロードしていたアセットを取得する
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public AssetLoadResult<T> GetPreloadAsset<T>(string key) where T : UnityEngine.Object
    {
        // キャッシュにアセットがあればそれを返す
        var cached = _Cache.GetFromCache(key);
        if (cached.HasValue && cached.Value.Status == AsyncOperationStatus.Succeeded)
        {
            return new AssetLoadResult<T>(cached.Value.Result as T);
        }
        else if (cached.HasValue)
        {
            return new AssetLoadResult<T>(AssetLoadErrorType.Exception, cached.Value.OperationException?.Message);
        }

        return new AssetLoadResult<T>(AssetLoadErrorType.NotFound, $"Asset not found in preload cache: {key}");
    }
    
}

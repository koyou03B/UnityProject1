using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressablesAssetPreloader : BaseAssetLoader
{
    public AddressablesAssetPreloader(ILogger logger) : base(logger) { }

    /// <summary>
    /// アセットを事前にロードしてキャッシュに保存
    /// </summary>
    /// <typeparam name="T">ロードする型</typeparam>
    /// <param name="key">キー</param>
    public async Task PreloadAsset<T>(string key, bool retryIfReleasing = true) where T : UnityEngine.Object
    {
        if (retryIfReleasing)
        {
            //数回の猶予をもって解放まで待ってみる
            var success = await WaitUntilReleasedAsync(key);
            if (!success)
            {
                _Logger.LogWarning($"Asset is still being released after retries: {key}");
            }
        }
        //このタイミング解放されてないならアウト
        else if (IsReleasing(key))
        {
            _Logger.LogWarning($"Asset is currently being released: {key}");
        }

        try
        {
            // キャッシュに既にあるか確認
            if (_Cache.IsCacheAvailable(key)) return;

            // アセットを非同期で読み込み
            var op = Addressables.LoadAssetAsync<T>(key);
            op.Completed += (handle) =>
            {
                //完了時にキャッシュ追加
                _Cache.AddToCache(key, handle);
            };
            await op.Task;
        }
        catch(Exception e)
        {
            _Logger.LogError($"Exception during load: {e.Message}");
        }
    }

    /// <summary>
    /// アセットを事前にロードしてキャッシュに保存
    /// </summary>
    /// <typeparam name="T">ロードする型</typeparam>
    /// <param name="key">キー</param>
    public async Task PreloadAssetWithProgressAsync<T>(string key, IProgressReporter progress = null, bool retryIfReleasing = true) where T : UnityEngine.Object
    {
        if (retryIfReleasing)
        {
            //数回の猶予をもって解放まで待ってみる
            var success = await WaitUntilReleasedAsync(key);
            if (!success)
            {
                _Logger.LogWarning($"Asset is still being released after retries: {key}");
            }
        }
        //このタイミング解放されてないならアウト
        else if (IsReleasing(key))
        {
            _Logger.LogWarning($"Asset is currently being released: {key}");
        }

        try
        {
            // キャッシュに既にあるか確認
            if (_Cache.IsCacheAvailable(key)) return;

            // アセットを非同期で読み込み
            var op = Addressables.LoadAssetAsync<T>(key);
            while (!op.IsDone)
            {
                progress?.Report(op.PercentComplete);
                await Task.Yield(); // フレームを跨いでループ
            }
            //完了時にキャッシュ追加
            _Cache.AddToCache(key, op);
        }
        catch (Exception e)
        {
            _Logger.LogError($"Exception during load: {e.Message}");
        }
    }

    /// <summary>
    /// ラベルに紐づくアセットを個別に非同期ロードしてキャッシュする
    /// </summary>
    /// <typeparam name="T">ロード対象の型</typeparam>
    /// <param name="label">Addressableのラベル</param>
    public async Task PreloadLabelAssetsIndividuallyAsync<T>(string label, bool retryIfReleasing = true) where T : UnityEngine.Object
    {
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
                    }
                }
                //このタイミング解放されてないならアウト
                else if (IsReleasing(location.PrimaryKey))
                {
                    _Logger.LogWarning($"Asset is currently being released: {location.PrimaryKey}");
                }

                // アセットを非同期で読み込み
                var op = Addressables.LoadAssetAsync<T>(location);

                //Load後に行ってほしい処理
                async Task LoadAndCache()
                {
                    await op.Task; // ← これでメインスレッドに戻る(ここでいうメインは呼び出し元のスレッドのこと)

                    //成功時のみキャッシュに登録
                    if (op.Status == AsyncOperationStatus.Succeeded)
                    {
                        _Cache.AddToCache(location.PrimaryKey, op);
                    }
                    else
                    {
                        _Logger.LogWarning($"Failed to preload {location.PrimaryKey}: {op.OperationException}");
                    }
                }
                loadTasks.Add(LoadAndCache());
            }

            //全部アセットのロードが終わるまで待機
            await Task.WhenAll(loadTasks);
        }
        catch(Exception e)
        {
            _Logger.LogError($"Exception during load: {e.Message}");
        }
    }

    /// <summary>
    /// ラベルに紐づくアセットを個別に非同期ロードしてキャッシュする
    /// また、読み込みの進捗を渡す
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="label"></param>
    /// <param name="progress"></param>
    /// <returns></returns>
    public async Task PreloadLabelAssetsIndividuallyWithProgressAsync<T>(string label, IProgressReporter progress = null, bool retryIfReleasing = true) where T : UnityEngine.Object
    {
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
                    }
                }
                //このタイミング解放されてないならアウト
                else if (IsReleasing(location.PrimaryKey))
                {
                    _Logger.LogWarning($"Asset is currently being released: {location.PrimaryKey}");
                }

                // アセットを非同期で読み込み
                var op = Addressables.LoadAssetAsync<T>(location);

                //Load後に行ってほしい処理
                async Task LoadAndCache()
                {
                    await op.Task; // ← これでメインスレッドに戻る(ここでいうメインは呼び出し元のスレッドのこと)

                    //成功時のみキャッシュに登録
                    if (op.Status == AsyncOperationStatus.Succeeded)
                    {
                        _Cache.AddToCache(location.PrimaryKey, op);
                    }
                    else
                    {
                        _Logger.LogWarning($"Failed to preload {location.PrimaryKey}: {op.OperationException}");
                    }

                    //トータルに対して完了した数で進捗を送る
                    ++completed;
                    progress?.Report((float)completed / total);
                }
                loadTasks.Add(LoadAndCache());
            }

            //全部アセットのロードが終わるまで待機
            await Task.WhenAll(loadTasks);
        }
        catch (Exception e)
        {
            _Logger.LogError($"Exception during load: {e.Message}");
        }
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
            return new AssetLoadResult<T>(cached.Value.OperationException?.Message);
        }

        return new AssetLoadResult<T>($"Asset not found in preload cache: {key}");
    }
    
}

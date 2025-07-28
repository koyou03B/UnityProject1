using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


/// <summary>
/// ここでは読み込むことだけを考える
/// </summary>
public class AddressablesAssetLoader : BaseAssetLoader
{
    public AddressablesAssetLoader(ILogger logger) : base(logger) { }

    /// <summary>
    /// 非同期ロード
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public async ValueTask<AssetLoadResult<T>> LoadAssetAsync<T>(string key, bool retryIfReleasing = true,
        CancellationToken token = default) where T : UnityEngine.Object
    {
        if (retryIfReleasing)
        {
            //数回の猶予をもって解放まで待ってみる
            var success = await WaitUntilReleasedAsync(key);
            if (!success)
            {
                return new AssetLoadResult<T>(AssetLoadErrorType.InUse,$"Asset is still being released after retries: {key}");
            }
        }
        //このタイミング解放されてないならアウト
        else if (IsReleasing(key))
        {
            return new AssetLoadResult<T>(AssetLoadErrorType.InUse,$"Asset is currently being released: {key}");
        }

        try
        {
            //キャッシュがあればそっちを使う
            var cached = _Cache.GetFromCache(key);
            if (cached.HasValue)
            {
                return new AssetLoadResult<T>(cached.Value.Result as T);
            }

            //非同期で読み込み
            var op = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<T>(key);
            while (!op.IsDone)
            {
                //キャンセルを確認したら例外を吐く
                token.ThrowIfCancellationRequested();
                await Task.Yield();
            }
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                //成功時にキャッシュにいれる
                _Cache.AddToCache(key, op);
                return new AssetLoadResult<T>(op.Result as T);
            }

            return new AssetLoadResult<T>(AssetLoadErrorType.NotFound,$"Failed to load asset: {key}");
        }
        catch (OperationCanceledException)
        {
            return new AssetLoadResult<T>(AssetLoadErrorType.Canceled,"Canceled");
        }
        catch (Exception e)
        {
            return new AssetLoadResult<T>(AssetLoadErrorType.Exception, $"Exception during load: {e.Message}");
        }
    }

    /// <summary>
    /// 非同期ロードするアセットの読み込み進捗を渡す
    /// </summary>
    /// <typeparam name="T">ロード対象の型</typeparam>
    /// <param name="key">キー</param>
    /// <param name="progress">進捗関数</param>
    /// <returns></returns>
    public async ValueTask<AssetLoadResult<T>> LoadAssetWithProgressAsync<T>(string key,IProgressReporter progress = null, 
        bool retryIfReleasing = true, CancellationToken token = default) where T :UnityEngine.Object
    {
        if (retryIfReleasing)
        {
            //数回の猶予をもって解放まで待ってみる
            var success = await WaitUntilReleasedAsync(key);
            if (!success)
            {
                return new AssetLoadResult<T>(AssetLoadErrorType.InUse,$"Asset is still being released after retries: {key}");
            }
        }
        //このタイミング解放されてないならアウト
        else if (IsReleasing(key))
        {
            return new AssetLoadResult<T>(AssetLoadErrorType.InUse,$"Asset is currently being released: {key}");
        }

        try
        {
            // キャッシュにアセットがあればそっちを返す
            AsyncOperationHandle? cachedAsset = _Cache.GetFromCache(key);
            if (cachedAsset.HasValue)
            {
                //進捗完了ってことは1
                progress?.Report(1.0f);
                return new AssetLoadResult<T>(cachedAsset.Value.Result as T);
            }

            //非同期で読み込み
            var op = Addressables.LoadAssetAsync<T>(key);
            while (!op.IsDone)
            {
                //キャンセルが出たら例外に
                token.ThrowIfCancellationRequested();
                progress?.Report(op.PercentComplete);
                await Task.Yield(); // フレームを跨いでループ
            }

            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                progress?.Report(1.0f);
                //成功時にキャッシュにいれる
                _Cache.AddToCache(key, op);
                return new AssetLoadResult<T>(op.Result as T);
            }

            //キーが違う可能性
            return new AssetLoadResult<T>(AssetLoadErrorType.NotFound,$"Failed to load asset: {key}");
        }
        catch (OperationCanceledException)
        {
            return new AssetLoadResult<T>(AssetLoadErrorType.Canceled,"Canceled");
        }
        catch (Exception e)
        {
            return new AssetLoadResult<T>(AssetLoadErrorType.Exception, $"Exception during load: {e.Message}");
        }
    }

}

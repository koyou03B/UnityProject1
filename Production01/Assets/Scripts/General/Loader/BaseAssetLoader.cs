using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public abstract class BaseAssetLoader
{
    protected AssetCache _Cache = new();
    protected ILogger _Logger;

    protected virtual int RetryCount => 3;
    protected virtual int RetryFrameDelay => 1;

    /// <summary>
    /// 非同期解放処理
    /// </summary>
    /// <param name="key"></param>
    public async Task ReleaseAsync(string key)
    {
       await  _Cache.ClearCache(key);
        _Logger?.Log($"ReleasedAsync asset: {key}");
    }

    /// <summary>
    /// 非同期解放処理
    /// </summary>
    /// <returns></returns>
    public async Task ReleaseAllAsync()
    {
         await _Cache.ClearCache();
        _Logger?.Log("ReleasedAsync all assets.");
    }

    /// <summary>
    /// 解放処理
    /// </summary>
    /// <param name="key"></param>
    public void ReleaseImmediate(string key)
    {
        _Cache.ClearCache(key);
        _Logger?.Log($"Released asset: {key}");
    }

    /// <summary>
    /// 解放処理
    /// </summary>
    public void ReleaseAllImmediate()
    {
        _Cache.ClearCache();
        _Logger?.Log("Released all assets.");
    }


    /// <summary>
    /// ロードしているかどうか
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool IsLoaded(string key)
    {
        return _Cache.IsCacheAvailable(key);
    }

    protected BaseAssetLoader(ILogger logger)
    {
        _Logger = logger;
    }

    /// <summary>
    /// 解放中かどうか
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    protected bool IsReleasing(string key)
    {
        return _Cache.IsReleasingAssetKey(key);
    }

    /// <summary>
    /// 解放まで待つ
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    protected async Task<bool> WaitUntilReleasedAsync(string key)
    {
        //設定した回数分は挑戦
        for (int i = 0; i < RetryCount; i++)
        {
            if (!IsReleasing(key))
            {
                return true;
            }
            _Logger?.LogWarning($"Asset {key} is being released. Retrying in frame... ({i + 1}/{RetryCount})");
            await WaitForFramesAsync(RetryFrameDelay);
        }

        _Logger?.LogError($"Asset {key} is still being released after {RetryCount} retries.");
        return false;
    }

    /// <summary>
    /// 指定したフレーム分待機
    /// </summary>
    /// <param name="frameCount"></param>
    /// <returns></returns>
    protected async Task WaitForFramesAsync(int frameCount)
    {
        for (int i = 0; i < frameCount; i++)
        {
            await Task.Yield();
        }
    }
}

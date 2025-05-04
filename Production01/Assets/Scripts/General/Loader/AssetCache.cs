using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// アセットのキャッシュを保持しておく
/// できるだけ参照カウントを減らしたい
/// </summary>
public class AssetCache
{
    private Dictionary<string, AsyncOperationHandle> _dCache = new Dictionary<string, AsyncOperationHandle>();
    //使い方: https://qiita.com/kohi7777/items/e6264c960fc2e6980596
    private HashSet<string> _ReleaseAssetKeys = new HashSet<string>();
    private ILogger _Logger = new PrefixLogger(new UnityLogger(), "[AssetCache]");

    /// <summary>
    /// キャッシュにアセットを追加する
    /// </summary>
    /// <param name="key">キー</param>
    /// <param name="asset">登録するアセット</param>
    public void AddToCache(string key, AsyncOperationHandle handle)
    {
        //解放中なんで追加禁止です
        if(_ReleaseAssetKeys.Contains(key))
        {
            _Logger.LogWarning($"Cannot add to cache: {key} is currently being released.");
            return;
        }
        //キーがなければ追加
        if (_dCache.ContainsKey(key)) return;
        _dCache[key] = handle;
    }

    /// <summary>
    /// キャッシュ内にアセットが存在するか確認
    /// </summary>
    /// <param name="key">キー</param>
    /// <returns></returns>
    public bool IsCacheAvailable(string key)
    {
        return _dCache.ContainsKey(key);
    }

    /// <summary>
    /// 解放しているアセットなのかどうか
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool IsReleasingAssetKey(string key)
    {
        return _ReleaseAssetKeys.Contains(key);
    }

    /// <summary>
    /// キャッシュからアセットの取得
    /// AsyncOperationHandleは値型だったので
    /// ?をつけてNullableにしてnullが効くように
    /// </summary>
    /// <param name="key">キー</param>
    /// <returns></returns>
    public AsyncOperationHandle? GetFromCache(string key)
    {
        if (_ReleaseAssetKeys.Contains(key))
        {
            _Logger.LogWarning($"Asset with key {key} is currently being released.");
            return null;
        }
        if (!_dCache.ContainsKey(key))
        {
            _Logger.LogWarning($"Asset with key {key} not found in cache.");
            return null;
        }
        return _dCache[key];
    }

    /// <summary>
    /// キャッシュ内のアセットを削除
    /// </summary>
    public async Task ClearCache()
    {
        foreach (var key in new List<string>(_dCache.Keys))
        {
            _ReleaseAssetKeys.Add(key);
            Addressables.Release(_dCache[key]);
            //同じタイミングでロードと解放が行われるのを防止
            await Task.Yield();//1フレーム待機
            _dCache.Remove(key);
            _ReleaseAssetKeys.Remove(key);
        }

        _Logger.Log("Clear AllCache Completed");
    }

    /// <summary>
    /// 特定のキーのキャッシュを削除
    /// </summary>
    /// <param name="key">キー</param>
    public async Task ClearCache(string key)
    {
        if (_dCache.ContainsKey(key))
        {
            _ReleaseAssetKeys.Add(key);
            Addressables.Release(_dCache[key]);
            //同じタイミングでロードと解放が行われるのを防止
            await Task.Yield();//1フレーム待機
            _dCache.Remove(key);
            _ReleaseAssetKeys.Remove(key);
        }
        _Logger.Log($"Clear {key} Cache Completed");
    }
}

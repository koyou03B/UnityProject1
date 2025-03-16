using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using System.Threading.Tasks;
using UnityEngine.ResourceManagement.Exceptions;

public class AssetLoader : SingletonMonoBehavior<AssetLoader>
{
    //ゲーム終了時に削除する用
    private List<AsyncOperationHandle> _GameEndOpHandleList;
    //Scene変更時に削除する用
    private List<AsyncOperationHandle> _SceneEndOpHandleList;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnLoadInit()
    {
        GameObject gameObject = new GameObject();
        gameObject.name = typeof(AssetLoader).Name;
        gameObject.AddComponent<AssetLoader>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _GameEndOpHandleList = new List<AsyncOperationHandle>();
        _SceneEndOpHandleList = new List<AsyncOperationHandle>();

        DontDestroyOnLoad(this.gameObject);
    }

    private void OnDestroy()
    {
        ReleaseGameEndAsset();
        ReleaseSceneEndAsset();
    }

    /// <summary>
    /// アセットの同期ロード
    /// </summary>
    /// <typeparam name="T">欲しい型</typeparam>
    /// <param name="key">Addresableに灯篭くしているパス名</param>
    /// <param name="sceneRelease">シーン破棄時に解放するか</param>
    /// <returns></returns>
    public T LoadAsset<T>(string key, bool sceneRelease = true) where T : UnityEngine.Object
    {
        var opHandle = Addressables.LoadAssetAsync<T>(key);
        T loadAsset = opHandle.WaitForCompletion();

        if (sceneRelease)
        {
            _SceneEndOpHandleList.Add(opHandle);
        }
        else
        {
            _GameEndOpHandleList.Add(opHandle);
        }

        return loadAsset;
    }

    /// <summary>
    /// アセットの非同期ロード
    /// </summary>
    /// <typeparam name="T">欲しい型</typeparam>
    /// <param name="key">登録しているパス名</param>
    /// <param name="callBack">ロード後に行う関数</param>
    /// <param name="sceneRelease">シーン破棄時に解放するか</param>
    /// <returns></returns>
    public async Task LoadAssetAsync<T>(string key, Action<T> callBack, bool sceneRelease = true) where T : UnityEngine.Object
    {
        var opHandle = Addressables.LoadAssetAsync<T>(key);
        await opHandle.Task;

        string downloadError = GetDownloadError(opHandle);
        if (!string.IsNullOrEmpty(downloadError))
        {
            Debug.LogError(downloadError);
        }
        else
        {
            callBack?.Invoke(opHandle.Result);
            if (sceneRelease)
            {
                _SceneEndOpHandleList.Add(opHandle);
            }
            else
            {
                _GameEndOpHandleList.Add(opHandle);
            }
        }
    }

    //シーンロード中に読み込みさせない
    private bool _IsLoadScene;

    /// <summary>
    /// 同期シーンロード
    /// </summary>
    /// <param name="key">登録しているシーン名</param>
    /// <param name="sceneMode">シーンの種類</param>
    /// <param name="callback">何かすることがあれば</param>
    public void LoadScene(string key, LoadSceneMode sceneMode, Action callback)
    {
        IEnumerator LoadScene()
        {
            _IsLoadScene = true;
            var opHandle = Addressables.LoadSceneAsync(key, sceneMode);
            opHandle.Completed += SceneLoadComlete;
            //TODO:Input系の動きを抑制筆記すること
            //下記説明URL:https://docs.unity3d.com/ja/2018.4/ScriptReference/Application-backgroundLoadingPriority.html
            Application.backgroundLoadingPriority = ThreadPriority.High;
          
            yield return opHandle.WaitForCompletion();

            Application.backgroundLoadingPriority = ThreadPriority.Normal;
            //TODO:Input系の動きを抑制は木を筆記すること

            if(sceneMode == LoadSceneMode.Single)
            {
                ReleaseSceneEndAsset();
            }
            _IsLoadScene = false;
            callback?.Invoke();
        }

        if (!_IsLoadScene)
        {
            StartCoroutine(LoadScene());
        }
    }

    /// <summary>
    /// 非同期のシーンロード
    /// </summary>
    /// <param name="key">登録しているパス</param>
    /// <param name="sceneMode">シーンモード</param>
    /// <returns></returns>
    public AsyncOperationHandle<SceneInstance> LoadSceneAsync(string key, LoadSceneMode sceneMode)
    {
         var opHandle = Addressables.LoadSceneAsync(key, sceneMode);
        opHandle.Completed += SceneLoadComlete;
        return opHandle;
    }

    /// <summary>
    /// シーン読み込み終わりのタイミングでリソース破棄させる
    /// </summary>
    /// <param name="obj"></param>
    private void SceneLoadComlete(AsyncOperationHandle<SceneInstance> opHandle)
    {
        ReleaseSceneEndAsset();
        opHandle.Completed -= SceneLoadComlete;
    }

    /// <summary>
    /// ダウンロードエラーがの内容を返却する
    /// </summary>
    /// <param name="handle"></param>
    /// <returns>
    /// ダウンロードエラー内容を表す文字列（https://docs.unity.cn/Packages/com.unity.addressables@1.20/manual/LoadingAssetBundles.html）を返す。
    /// エラーが存在しない場合はnullを返却する。
    /// 参考資料 (https://dixq.net/forum/viewtopic.php?t=8576)
    /// </returns>
    private string GetDownloadError(AsyncOperationHandle handle)
    {
        if (handle.Status != AsyncOperationStatus.Failed)
            return null;

        // AsyncOperationHandle.OperationException か InnerException に
        // RemoteProviderException 型があった場合はダウンロードエラー
        var exception = handle.OperationException;
        while (exception != null)
        {
            if (exception is RemoteProviderException remoteException)
                return remoteException.WebRequestResult.Error;

            exception = exception.InnerException;
        }
        return null;
    }

    /// <summary>
    /// SceneEndリソースの破棄
    /// </summary>
    private void ReleaseSceneEndAsset()
    {
        for (int i = _SceneEndOpHandleList.Count - 1; i >= 0; i--)
        {
            _SceneEndOpHandleList[i].Release();
            _SceneEndOpHandleList.Remove(_SceneEndOpHandleList[i]);
        }
    }

    /// <summary>
    /// GameEndリソースの破棄
    /// </summary>
    private void ReleaseGameEndAsset()
    {
        for (int i = _GameEndOpHandleList.Count - 1; i >= 0; i--)
        {
            _GameEndOpHandleList[i].Release();
            _GameEndOpHandleList.Remove(_GameEndOpHandleList[i]);
        }
    }
}

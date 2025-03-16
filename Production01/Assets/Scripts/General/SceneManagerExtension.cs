using System;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public static class SceneManagerExtension
{

    /// <summary>
    /// 同期シーンロード
    /// </summary>
    /// <param name="key">登録しているシーン名</param>
    /// <param name="sceneMode">シーンの種類</param>
    /// <param name="callback">何かすることがあれば</param>
    public static void LoadScene(string key, LoadSceneMode sceneMode, Action callback)
    {
        AssetLoader.Instance.LoadScene(key, sceneMode, callback);
    }

    /// <summary>
    /// 非同期のシーンロード
    /// </summary>
    /// <param name="key">登録しているパス</param>
    /// <param name="sceneMode">シーンモード</param>
    /// <returns>することがあれば使ってあげる</returns>
    public static AsyncOperationHandle<SceneInstance> LoadSceneAsync(string key, LoadSceneMode sceneMode)
    {
        return AssetLoader.Instance.LoadSceneAsync(key, sceneMode);
    }

    /// <summary>
    /// シーンのアンロード
    /// </summary>
    /// <param name="scene"></param>
    public static void UnloadSceneAsync(Scene scene)
    {
        SceneManager.UnloadSceneAsync(scene);
    }

    /// <summary>
    /// アクティブにするシーンをセットする
    /// </summary>
    /// <param name="scene"></param>
    public static void SetActiveScene(Scene scene)
    {
        SceneManager.SetActiveScene(scene);
    }

    /// <summary>
    /// 欲しいシーンをもらう
    /// </summary>
    /// <param name="sceneName">シーンの名前</param>
    /// <returns>Sceneを返す</returns>
    public static Scene GetSceneByName(string sceneName)
    {
        return SceneManager.GetSceneByName(sceneName);
    }
}

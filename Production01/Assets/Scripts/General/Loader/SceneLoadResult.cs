using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class SceneLoadResult
{
    public SceneInstance SceneInstance { get; }
    public Scene Scene { get { return SceneInstance.Scene; } }
    public AssetLoadErrorType ErrorType { get; }

    public string ErrorMessage { get; }

    public bool IsSuccess => ErrorType == AssetLoadErrorType.None;
    public SceneLoadResult(SceneInstance sceneInstance)
    {
        SceneInstance = sceneInstance;
        ErrorType = AssetLoadErrorType.None;
    }
    public SceneLoadResult(AssetLoadErrorType type, string errorMessage)
    {
        ErrorType = type;
        ErrorMessage = errorMessage;
    }
}

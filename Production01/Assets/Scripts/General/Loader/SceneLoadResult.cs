using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class SceneLoadResult
{
    public SceneInstance SceneInstance { get; }
    public Scene Scene { get { return SceneInstance.Scene; } }

    public string ErrorMessage { get; }

    public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage);

    public SceneLoadResult(SceneInstance sceneInstance) => SceneInstance = sceneInstance;
    public SceneLoadResult(string errorMessage) => ErrorMessage = errorMessage;
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class SingletonMonoBehavior<T> : MonoBehaviour  where T: Component
{
    private static T s_Instance;
    private static ILogger _Logger = new PrefixLogger(new UnityLogger(), "[Singlton]");
    private static readonly object _lock = new object();

    public static T Instance
    {
        get
        {
            lock (_lock)
            {
                if (s_Instance == null)
                {
                    s_Instance = (T)FindFirstObjectByType(typeof(T));
                    if (s_Instance == null)
                    {
                        SetupInstance();
                    }
                    else
                    {
                        _Logger.Log($"{typeof(T).Name} instance already exists: {s_Instance.gameObject.name}");
                    }
                }

                return s_Instance;
            }
        }
    }

    private static void SetupInstance()
    {
        GameObject obj = new GameObject(typeof(T).Name);
        s_Instance = obj.AddComponent<T>();
    }

    protected virtual void OnDestroy()
    {
        //同一のインスタンス化の確認
        if (ReferenceEquals(s_Instance, this))
        {
            s_Instance = null;
        }
    }
}

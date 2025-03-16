using UnityEngine;
using UnityEngine.SceneManagement;

public class SingletonMonoBehavior<T> : MonoBehaviour  where T: Component
{
    private static T s_Instance;

    public static T Instance
    {
        get
        {
            if(s_Instance == null)
            {
                s_Instance = (T)FindFirstObjectByType(typeof(T));
                if(s_Instance == null)
                {
                    SetupInstance();
                }
                else
                {
                    string typeName = typeof(T).Name;

                    Debug.Log("[Singleton] " + typeName + " instance already created: " +
                              s_Instance.gameObject.name);
                }
            }

            return s_Instance;
        }
    }

    private void OnDestroy()
    {
        if (s_Instance != null)
        {
            Destroy(s_Instance.gameObject);
        }
        s_Instance = null;
    }

    private static void SetupInstance()
    {
        GameObject gameObject = new GameObject();
        gameObject.name = typeof(T).Name;

        s_Instance = gameObject.AddComponent<T>();
    }
}

using UnityEngine;

public class GlobalAssetController : MonoBehaviour
{
    private readonly string CursorCanvasPath = "Prefabs/CursorCanvas";
    private GameObject _CursorCanvasObject;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnLoadInit()
    {
        GameObject gameObject = new GameObject();
        gameObject.name = typeof(GlobalAssetController).Name;
        gameObject.AddComponent<GlobalAssetController>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var cursorCanvasData = AssetLoader.Instance.LoadAsset<GameObject>(CursorCanvasPath, false);
        _CursorCanvasObject = GameObject.Instantiate(cursorCanvasData);

        DontDestroyOnLoad(this.gameObject);
    }

    //// Update is called once per frame
    //void Update()
    //{
        
    //}
}

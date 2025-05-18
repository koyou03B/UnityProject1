using System.Threading.Tasks;
using UnityEngine;

[DefaultExecutionOrder(-10)]
public class GlobalAssetController : MonoBehaviour
{
    private readonly string CursorCanvasPath = "Prefabs/CursorCanvas";
    private GameObject _CursorCanvasObject;
    private ILogger _Logger;
    private Observable<InputSystemKeyCode.eInputKeyType> _InputSystemObservable;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnLoadInit()
    {
        GameObject gameObject = new GameObject();
        gameObject.name = typeof(GlobalAssetController).Name;
        gameObject.AddComponent<GlobalAssetController>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async Task Start()
    {
        _Logger = new PrefixLogger(new UnityLogger(), "[GlobalAssetController]");
        var pleLoader = AssetLoaderService.Instance.AssetPreloder;
        await pleLoader.PreloadAsset<GameObject>(CursorCanvasPath);
        var cursorCanvasData = pleLoader.GetPreloadAsset<GameObject>(CursorCanvasPath);
        if(cursorCanvasData.IsSuccess)
        {
            _CursorCanvasObject = GameObject.Instantiate(cursorCanvasData.Asset);
        }
        else
        {
            _Logger.LogError(cursorCanvasData.ErrorMessage);
        }

        //いまだけ
        _InputSystemObservable = new Observable<InputSystemKeyCode.eInputKeyType>();
        _InputSystemObservable.RegistObserver(InputSystemController.Instance, InputSystemController.Instance.GetHashCode());
        _InputSystemObservable.SendNotify(InputSystemKeyCode.eInputKeyType.Game);

        DontDestroyOnLoad(this.gameObject);
    }

    //// Update is called once per frame
    void Update()
    {
        InputSystemController.Instance.Tick();
    }
}

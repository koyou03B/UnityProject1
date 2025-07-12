using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Threading.Tasks;

public enum eCursorMode
{
    Attack,    //敵と被っているとき
    Point,      //攻撃位置
    Base,       //基本の方
}

public class UICursorController : MonoBehaviour
{
    [SerializeField]
    private eCursorMode _DebugCursorMode;

    private readonly string MouseCursorDataPath = "ScriptableObjects/MouseCursorData";
    private readonly string CursorSpriteDataPath = "ScriptableObjects/CursorSpriteData";

    private MouseCursorData _MouseCursorData;
    private SpriteListData _CursorSpriteData;
    private ILogger _Logger;

    private Image _CursorImage;
    private Image _MouseInputImage;
    private RectTransform _SelfRectTransform;

    private void Awake()
    {
        _SelfRectTransform = GetComponent<RectTransform>();
        _CursorImage = GetComponent<Image>();
        _MouseInputImage = _SelfRectTransform.GetChild(0). GetComponent<Image>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async Task Start()
    {
        _Logger = new PrefixLogger(new UnityLogger(), "[Cursor]");

        var pleLoader = AssetLoaderService.Instance.AssetPreloder;
        await pleLoader.PreloadAsset<MouseCursorData>(MouseCursorDataPath);
        var mouseCursorData = pleLoader.GetPreloadAsset<MouseCursorData>(MouseCursorDataPath);
        if(mouseCursorData.IsSuccess)
        {
            _MouseCursorData = mouseCursorData.Asset;
            var cursorData = _MouseCursorData.GetMouseCursorInfo(eCursorMode.Base);
            _CursorImage.sprite = cursorData.CursorSprite;
            _SelfRectTransform.pivot = cursorData.Pivot;
        }
        else
        {
            _Logger.LogError(mouseCursorData.ErrorMessage);
        }

        await pleLoader.PreloadAsset<SpriteListData>(CursorSpriteDataPath);
        var cursorSpriteData = pleLoader.GetPreloadAsset<SpriteListData>(CursorSpriteDataPath);
        if(cursorSpriteData.IsSuccess)
        {
            _CursorSpriteData = cursorSpriteData.Asset;
        }
        _MouseInputImage.enabled = false;
        DontDestroyOnLoad(_SelfRectTransform.parent.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        _SelfRectTransform.anchoredPosition = InputSystemController.Instance.InputMouse.CursorPosition;
    }

    public void ChangeCursorMode(eCursorMode mode)
    {
        var cursorData = _MouseCursorData.GetMouseCursorInfo(mode);
        _CursorImage.sprite = cursorData.CursorSprite;
        _SelfRectTransform.pivot = cursorData.Pivot;

        if(mode == eCursorMode.Base)
        {
            _MouseInputImage.sprite = null;
            _MouseInputImage.enabled = false;
        }
        else
        {
            var mouseInputData = _CursorSpriteData.GetSprite((int)mode);
            _MouseInputImage.sprite = mouseInputData;
            _MouseInputImage.enabled = true;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using MiniJSON;

//優先的に処理されてほしい
[DefaultExecutionOrder(-10)]
public class InputSystemController : SingletonMonoBehavior<InputSystemController>
{
    private InputSystemKeyboard _InputKeyboard;
    private InputSystemMouse _InputSystemMouse;
    private Dictionary<InputSystemKeyCode.eInputSystemKeyCode, Key> _dCustomKeyCode;
    private string _CustomKeyCodeData = "";
    private bool _IsSetDefaultKeyCode = false;

    public InputSystemMouse InputMouse { get { return _InputSystemMouse; } private set { _InputSystemMouse = value; } }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnLoadInit()
    {
        GameObject gameObject = new GameObject();
        gameObject.name = typeof(InputSystemController).Name;
        gameObject.AddComponent<InputSystemController>();
    }

    private void Awake()
    {
        _InputKeyboard = gameObject.AddComponent<InputSystemKeyboard>();
        _InputSystemMouse = gameObject.AddComponent<InputSystemMouse>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CustomKeyCodeInit();

        _InputKeyboard.OnKeyboardInit();
        _InputSystemMouse.OnMouseInit();

        DontDestroyOnLoad(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        _InputKeyboard.CheckConnectKeyboard();
        _InputSystemMouse.OnMouseUpdate();
    }

    /// <summary>
    /// カスタムキーコードをロードする
    /// </summary>
    /// <param name="jsonStr"></param>
    public void LoadCustomKeyCode(string jsonStr)
    {
        //初期化対応が終わっていないなら受け取って終わる
        if(!_IsSetDefaultKeyCode)
        {
            _CustomKeyCodeData = jsonStr;
            return;
        }
 
        //取得したファイルを複合化
        var jsonData = Json.Deserialize(jsonStr) as Dictionary<InputSystemKeyCode.eInputSystemKeyCode, Key>;
        if(jsonData != null)
        {
            _dCustomKeyCode = jsonData;
        }
    }

    /// <summary>
    /// カスタムキーコードの保存
    /// </summary>
    /// <returns></returns>
    public string SaveCustomKeyCode()
    {
        _CustomKeyCodeData =  Json.Serialize(_dCustomKeyCode);
        return _CustomKeyCodeData;
    }
    /// <summary>
    /// 指定のキーを押しているかどうか
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool GetKeyDown(InputSystemKeyCode.eInputSystemKeyCode key,InputSystemKeyCode.eInputKeyType type = InputSystemKeyCode.eInputKeyType.Game)
    {
        if(type == InputSystemKeyCode.eInputKeyType.Game)
        {
            return _InputKeyboard.GetKeyDown(_dCustomKeyCode[key]);
        }   

        //UI用
        return _InputKeyboard.GetKeyDown(key);
    }

    /// <summary>
    /// 指定のキーを離したかどうか
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool GetKeyUp(InputSystemKeyCode.eInputSystemKeyCode key, InputSystemKeyCode.eInputKeyType type = InputSystemKeyCode.eInputKeyType.Game)
    {
        if(type == InputSystemKeyCode.eInputKeyType.Game)
        {
            return _InputKeyboard.GetKeyUp(_dCustomKeyCode[key]);

        }
        return _InputKeyboard.GetKeyUp(key);
    }

    /// <summary>
    /// 指定のキーを押されているか
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool GetKeyPress(InputSystemKeyCode.eInputSystemKeyCode key, InputSystemKeyCode.eInputKeyType type = InputSystemKeyCode.eInputKeyType.Game)
    {
        if (type == InputSystemKeyCode.eInputKeyType.Game)
        {
            return _InputKeyboard.GetKeyPress(_dCustomKeyCode[key]);

        }
        return _InputKeyboard.GetKeyPress(key);
    }

    /// <summary>
    /// カスタムキーコードの設定
    /// </summary>
    private void CustomKeyCodeInit()
    {
        _dCustomKeyCode = new Dictionary<InputSystemKeyCode.eInputSystemKeyCode, Key>();
        foreach (var keyCode in InputSystemKeyCode.DefaultKeyCode)
        {
            _dCustomKeyCode.Add(keyCode.Key, keyCode.Value);
        }
        _IsSetDefaultKeyCode = true;
        //カスタムデータを受け取っていたらロードする
        if (_CustomKeyCodeData.Contains("") == false)
        {
            LoadCustomKeyCode(_CustomKeyCodeData);
        }
    }
}

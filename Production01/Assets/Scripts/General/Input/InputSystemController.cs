using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using MiniJSON;

//優先的に処理されてほしい
[DefaultExecutionOrder(-10)]
public class InputSystemController : SingletonMonoBehavior<InputSystemController>,IObserver<InputSystemKeyCode.eInputKeyType>
{
    private InputContext[] _InputContexts;
    private InputContext _CurrentInput;
    private InputSystemMouse _Mouse;
    private ILogger _Logger;

    public bool _IsInputLock;

    public InputSystemMouse InputMouse { get { return _Mouse; } private set { _Mouse = value; } }
    public bool IsInputLock { private get { return _IsInputLock; } set { _IsInputLock = value; } }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitOnBoot()
    {
        _ = InputSystemController.Instance;
    }

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public void Start()
    {
        _InputContexts = new InputContext[2];
        _InputContexts[0] = new InputContext(InputSystemKeyCode.eInputKeyType.Game);
        _InputContexts[1] = new InputContext(InputSystemKeyCode.eInputKeyType.UI);
        _CurrentInput = null;

        _Mouse = new InputSystemMouse();
        _Logger = new PrefixLogger(new UnityLogger(), "[InputSystemController]");
        _IsInputLock = false;
    }

    public void OnNotify(InputSystemKeyCode.eInputKeyType state)
    {
        for (int i = 0; i < _InputContexts.Length; i++)
        {
            if (state == _InputContexts[i].KeyType)
            {
                _CurrentInput = _InputContexts[i];
                break;
            }
        }
    }

    public void OnError(System.Exception error)
    {
        _Logger.LogError($"Exception during load: {error.Message}");
    }


    public void Tick()
    {
        if(_CurrentInput != null)
        {
            _CurrentInput.Tick();
        }
        _Mouse.OnMouseUpdate();
    }

    /// <summary>
    /// Loadしたキーパックをセットする
    /// </summary>
    public bool LoadCustomKeyCodePack(byte[] packedData)
    {
        if (!BytePacker.TryUnpack(packedData, out byte type, out byte version, out List<int> keyPack))
        {
            _Logger.LogError("Failed to unpack input data.");
            return false;
        }

        if (type != 1)
        {
            _Logger.LogError($"Unexpected type: {type}");
            return false;
        }

        // ここで、バージョンによってUseInputControllerに任せる
        return _InputContexts[0].LoadKeyPack(version, keyPack);
    }

    /// <summary>
    /// カスタムキーコードの保存
    /// </summary>
    /// <returns></returns>
    public byte[] SaveCustomKeyCodePack()
    {
        return _InputContexts[0].GetCustomKeyCodePack();
    }
    /// <summary>
    /// 指定のキーを押しているかどうか
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool GetKeyDown(InputSystemKeyCode.eInputSystemKeyCode key)
    {
        if(_IsInputLock)
        {
            _Logger.Log("Currently not accepting input");
            return false;
        }
        if (_CurrentInput == null)
        {
            _Logger.LogWarning($"{_CurrentInput} is null");
            return false;
        }

        return _CurrentInput.GetKeyDown(key);
    }

    /// <summary>
    /// 指定のキーを離したかどうか
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool GetKeyUp(InputSystemKeyCode.eInputSystemKeyCode key)
    {
        if (_IsInputLock)
        {
            _Logger.Log("Currently not accepting input");
            return false;
        }
        if (_CurrentInput == null)
        {
            _Logger.LogWarning($"{_CurrentInput} is null");
            return false;
        }

        return _CurrentInput.GetKeyUp(key);
    }

    /// <summary>
    /// 指定のキーを押されているか
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool GetKeyPress(InputSystemKeyCode.eInputSystemKeyCode key)
    {
        if (_IsInputLock)
        {
            _Logger.Log("Currently not accepting input");
            return false;
        }

        if(_CurrentInput == null)
        {
            _Logger.LogWarning($"{_CurrentInput} is null");
            return false;
        }

        return _CurrentInput.GetKeyPress(key);
    }


}

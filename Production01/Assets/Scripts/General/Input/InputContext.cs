using System.Collections.Generic;
using UnityEngine;
using static InputSystemKeyCode;

public class InputContext
{
    private eInputKeyType _eInputKeyType;
    private ILogger _Logger;

    private InputSystemKeyboard _Keyboard;
    private CustomInputKey _CustomInputKey;

    public InputContext(eInputKeyType  keyType)
    {
        this._eInputKeyType = keyType;
        _Logger = new PrefixLogger(new UnityLogger(), "[UseInputController]");

        _Keyboard = new InputSystemKeyboard();

        bool isLock = keyType == eInputKeyType.UI;
        _CustomInputKey = new CustomInputKey(isLock);
    }

    public eInputKeyType KeyType => _eInputKeyType;


    public void Tick()
    {
        _Keyboard.Tick();
    }

    /// <summary>
    /// キーが押されたとき
    /// </summary>
    /// <param name="keyCode"></param>
    /// <returns></returns>
    public bool GetKeyDown(eInputSystemKeyCode keyCode)
    {
        var key = _CustomInputKey.GetKey(keyCode);
        return _Keyboard.GetKeyDown(key);
    }

    /// <summary>
    /// キーが離されたかどうか
    /// </summary>
    /// <param name="keyCode"></param>
    /// <returns></returns>
    public bool GetKeyUp(eInputSystemKeyCode keyCode)
    {
        var key = _CustomInputKey.GetKey(keyCode);
        return _Keyboard.GetKeyUp(key);
    }

    /// <summary>
    /// キーが押されているとき
    /// </summary>
    /// <param name="keyCode"></param>
    /// <returns></returns>
    public bool GetKeyPress(eInputSystemKeyCode keyCode)
    {
        var key = _CustomInputKey.GetKey(keyCode);
        return _Keyboard.GetKeyPress(key);
    }

    /// <summary>
    /// Save用のbyte配列でカスタムキーを渡す
    /// </summary>
    /// <returns></returns>
    public byte[] GetCustomKeyCodePack()
    {
        List<int> keyPack = _CustomInputKey.CreateSavePackKeyCode();
        return BytePacker.Pack(0,0,keyPack);
    }

    /// <summary>
    /// Loadしたデータを当てはめてもらう
    /// </summary>
    /// <param name="version"></param>
    /// <param name="rawData"></param>
    public bool LoadKeyPack(byte version, List<int> keyPack)
    {
        switch (version)
        {
            case 0:
                _CustomInputKey.SetKeyCodePack(keyPack);
                return true;

            default:
                _Logger.LogError($"Unsupported version: {version}");
                return false;
        }
    }
}

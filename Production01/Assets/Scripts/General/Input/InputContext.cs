using System.Collections.Generic;
using UnityEngine;
using static InputSystemKeyCode;

public class InputContext
{
    private eInputKeyType _eInputKeyType;
    private ILogger _Logger;

    private InputSystemKeyboard _Keyboard;
    private CustomInputKey _CustomInputKey;
    private CustomInputKeyPacker _CustomInputKeyPacker;
    public InputContext(eInputKeyType  keyType)
    {
        this._eInputKeyType = keyType;
        _Logger = new PrefixLogger(new UnityLogger(), "[InputContext]");

        _Keyboard = new InputSystemKeyboard();

        bool isLock = keyType == eInputKeyType.UI;
        _CustomInputKey = new CustomInputKey(isLock);
        _CustomInputKeyPacker = new CustomInputKeyPacker();
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

    private readonly byte _Version = 0;
    /// <summary>
    /// Save用のbyte配列でカスタムキーを渡す
    /// </summary>
    /// <returns></returns>
    public byte[] GetCustomKeyCodePack()
    {
        List<int> keyPack = _CustomInputKey.CreateSavePackKeyCode();
        byte[] payload =  _CustomInputKeyPacker.PackPayload(keyPack, _Version);

        return BytePacker.Pack((byte)SaveLoadEnum.eSaveType.Input, _Version, payload);
    }

    /// <summary>
    /// Loadしたデータを当てはめてもらう
    /// </summary>
    /// <param name="version"></param>
    /// <param name="rawData"></param>
    public bool LoadKeyPack(byte version, byte[] payload)
    {
       bool completeUnpacked =  _CustomInputKeyPacker.TryUnpackPayload(payload, version, out List<int> keyPack);
       if(completeUnpacked)
        {
            _CustomInputKey.SetKeyCodePack(keyPack);
        }
        return completeUnpacked;
    }
}

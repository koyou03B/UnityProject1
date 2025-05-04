using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystemKeyboard 
{
    private Keyboard _CurrentKeyboard;
    private ILogger _Logger;
    private bool _IsConnectKeyboard;

    public InputSystemKeyboard()
    {
        _CurrentKeyboard = Keyboard.current;
        _Logger = new PrefixLogger(new UnityLogger(), "[Keyboard]");
        _IsConnectKeyboard = (_CurrentKeyboard == null) ? false : true;
    }

    public void Tick()
    {
        CheckConnectKeyboard();
    }
    /// <summary>
    /// 接続確認
    /// </summary>
    private void CheckConnectKeyboard()
    {
        var keyboard = Keyboard.current;
        if (keyboard != _CurrentKeyboard)
        {
            _CurrentKeyboard = keyboard;
            _IsConnectKeyboard = (_CurrentKeyboard == null) ? false : true;
        }
    }


    /// <summary>
    /// キーが押されたとき
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool GetKeyDown(Key key)
    {
        if (!_IsConnectKeyboard)
        {
            _Logger.LogWarning("Thie Keyboard is not connecting");
            return false;
        }

        return _CurrentKeyboard?[key]?.wasPressedThisFrame ?? false;
    }

    /// <summary>
    /// キーが離されたかどうか
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool GetKeyUp(Key key)
    {
        if (!_IsConnectKeyboard)
        {
            _Logger.LogWarning("Thie Keyboard is not connecting");
            return false;
        }
        return _CurrentKeyboard?[key]?.wasReleasedThisFrame ?? false;
    }

    /// <summary>
    /// キーが押されているとき
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool GetKeyPress(Key key)
    {
        if (!_IsConnectKeyboard)
        {
            _Logger.LogWarning("Thie Keyboard is not connecting");
            return false;
        }
        return _CurrentKeyboard?[key]?.isPressed ?? false;
    }


}

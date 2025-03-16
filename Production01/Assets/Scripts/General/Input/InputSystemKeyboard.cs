using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystemKeyboard : MonoBehaviour
{
    private Keyboard _CurrentKeyboard;
    private bool _IsConnectKeyboard;

    /// <summary>
    /// 初期化
    /// </summary>
   public  void OnKeyboardInit()
    {
        _CurrentKeyboard = Keyboard.current;
        _IsConnectKeyboard = (_CurrentKeyboard == null) ? false : true;
    }

    /// <summary>
    /// 接続確認
    /// </summary>
    public void CheckConnectKeyboard()
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
        if (!_IsConnectKeyboard) return false;

        var targetKey = _CurrentKeyboard[key];
        return targetKey.wasPressedThisFrame;
    }

    /// <summary>
    /// UI用のKeyDown
    /// </summary>
    /// <param name="keyCode"></param>
    /// <returns></returns>
    public bool GetKeyDown(InputSystemKeyCode.eInputSystemKeyCode keyCode)
    {
        if (!_IsConnectKeyboard) return false;
        if(InputSystemKeyCode.DefaultKeyCode.ContainsKey(keyCode))
        {
            Key key = InputSystemKeyCode.DefaultKeyCode[keyCode];
            var targetKey = _CurrentKeyboard[key];
            return targetKey.wasPressedThisFrame;
        }

        //Move系なら二種から確認する
        bool wasPressed = keyCode switch
        {
            InputSystemKeyCode.eInputSystemKeyCode.MoveUp => _CurrentKeyboard[Key.W].wasPressedThisFrame || _CurrentKeyboard[Key.UpArrow].wasPressedThisFrame,
            InputSystemKeyCode.eInputSystemKeyCode.MoveDown => _CurrentKeyboard[Key.S].wasPressedThisFrame || _CurrentKeyboard[Key.DownArrow].wasPressedThisFrame,
            InputSystemKeyCode.eInputSystemKeyCode.MoveRight => _CurrentKeyboard[Key.D].wasPressedThisFrame || _CurrentKeyboard[Key.RightArrow].wasPressedThisFrame,
            InputSystemKeyCode.eInputSystemKeyCode.MoveLeft => _CurrentKeyboard[Key.A].wasPressedThisFrame || _CurrentKeyboard[Key.LeftArrow].wasPressedThisFrame,
            _ => false,
        };

        return wasPressed;
    }

    /// <summary>
    /// キーが離されたかどうか
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool GetKeyUp(Key key)
    {
        if (!_IsConnectKeyboard) return false;

        var targetKey = _CurrentKeyboard[key];
        return targetKey.wasReleasedThisFrame;
    }

    /// <summary>
    /// Ui用のKeyUp
    /// </summary>
    /// <param name="keyCode"></param>
    /// <returns></returns>
    public bool GetKeyUp(InputSystemKeyCode.eInputSystemKeyCode keyCode)
    {
        if (!_IsConnectKeyboard) return false;
        if (InputSystemKeyCode.DefaultKeyCode.ContainsKey(keyCode))
        {
            Key key = InputSystemKeyCode.DefaultKeyCode[keyCode];
            var targetKey = _CurrentKeyboard[key];
            return targetKey.wasReleasedThisFrame;
        }

        //Move系なら二種から確認する
        bool wasReleased = keyCode switch
        {
            InputSystemKeyCode.eInputSystemKeyCode.MoveUp => _CurrentKeyboard[Key.W].wasReleasedThisFrame || _CurrentKeyboard[Key.UpArrow].wasReleasedThisFrame,
            InputSystemKeyCode.eInputSystemKeyCode.MoveDown => _CurrentKeyboard[Key.S].wasReleasedThisFrame || _CurrentKeyboard[Key.DownArrow].wasReleasedThisFrame,
            InputSystemKeyCode.eInputSystemKeyCode.MoveRight => _CurrentKeyboard[Key.D].wasReleasedThisFrame || _CurrentKeyboard[Key.RightArrow].wasReleasedThisFrame,
            InputSystemKeyCode.eInputSystemKeyCode.MoveLeft => _CurrentKeyboard[Key.A].wasReleasedThisFrame || _CurrentKeyboard[Key.LeftArrow].wasReleasedThisFrame,
            _ => false,
        };

        return wasReleased;
    }
    /// <summary>
    /// キーが押されているとき
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool GetKeyPress(Key key)
    {
        if (!_IsConnectKeyboard) return false;

        var targetKey = _CurrentKeyboard[key];
        return targetKey.isPressed;
    }

    /// <summary>
    /// UI用のKeyPress
    /// </summary>
    /// <param name="keyCode"></param>
    /// <returns></returns>
    public bool GetKeyPress(InputSystemKeyCode.eInputSystemKeyCode keyCode)
    {
        if (!_IsConnectKeyboard) return false;
        if (InputSystemKeyCode.DefaultKeyCode.ContainsKey(keyCode))
        {
            Key key = InputSystemKeyCode.DefaultKeyCode[keyCode];
            var targetKey = _CurrentKeyboard[key];
            return targetKey.isPressed;
        }

        //Move系なら二種から確認する
        bool isPressed = keyCode switch
        {
            InputSystemKeyCode.eInputSystemKeyCode.MoveUp => _CurrentKeyboard[Key.W].isPressed || _CurrentKeyboard[Key.UpArrow].isPressed,
            InputSystemKeyCode.eInputSystemKeyCode.MoveDown => _CurrentKeyboard[Key.S].isPressed || _CurrentKeyboard[Key.DownArrow].isPressed,
            InputSystemKeyCode.eInputSystemKeyCode.MoveRight => _CurrentKeyboard[Key.D].isPressed || _CurrentKeyboard[Key.RightArrow].isPressed,
            InputSystemKeyCode.eInputSystemKeyCode.MoveLeft => _CurrentKeyboard[Key.A].isPressed || _CurrentKeyboard[Key.LeftArrow].isPressed,
            _ => false,
        };

        return isPressed;
    }

}

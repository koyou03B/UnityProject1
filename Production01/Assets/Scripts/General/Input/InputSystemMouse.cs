using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class InputSystemMouse 
{
    private Mouse _CurrentMouse;
    private ILogger _Logger;

    private Vector2 _CursorPosition;
    private bool _IsConnectMouse;

    public Vector2 CursorPosition { get { return _CursorPosition; } private set { _CursorPosition = value; } }

    public InputSystemMouse()
    {
        _CurrentMouse = Mouse.current;
        _Logger = new PrefixLogger(new UnityLogger(), "[Mouse]");
        _CursorPosition = Vector2.zero;
        _IsConnectMouse = (_CurrentMouse == null) ? false : true;

        //Cursor.visible = false;
        //Cursor.lockState = CursorLockMode.Confined; //これワンちゃんいらない明日見る
    }

    /// <summary>
    /// 更新処理
    /// </summary>
    public void OnMouseUpdate()
    {
        CheckConnectMouse();
        if(_IsConnectMouse)
        {
            _CursorPosition = _CurrentMouse.position.ReadValue();
        }
    }

    /// <summary>
    /// マウスクリックが押されたとき
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool GetKeyDown(InputSystemKeyCode.eInputMouseButton button)
    {
        if (!_IsConnectMouse)
        {
            _Logger.LogWarning("Thie Mouse is not connecting");
            return false;
        }

        return GetButton(button)?.wasPressedThisFrame ?? false;
    }

    /// <summary>
    /// マウスクリックが離されたとき
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool GetMouseUp(InputSystemKeyCode.eInputMouseButton button)
    {
        if (!_IsConnectMouse)
        {
            _Logger.LogWarning("Thie Mouse is not connecting");
            return false;
        }

        return GetButton(button)?.wasReleasedThisFrame ?? false;
    }


    /// <summary>
    /// マウスクリックが押されているかどうか
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool GetMousePress(InputSystemKeyCode.eInputMouseButton button)
    {
        if (!_IsConnectMouse)
        {
            _Logger.LogWarning("Thie Mouse is not connecting");
            return false;
        }

        return GetButton(button)?.isPressed ?? false;
    }

    /// <summary>
    /// 対応するボタンを入手
    /// </summary>
    /// <param name="button"></param>
    /// <returns></returns>
    private ButtonControl GetButton(InputSystemKeyCode.eInputMouseButton button)
    {
        return button switch
        {
            InputSystemKeyCode.eInputMouseButton.LeftButton => _CurrentMouse.leftButton,
            InputSystemKeyCode.eInputMouseButton.RightButton => _CurrentMouse.rightButton,
            InputSystemKeyCode.eInputMouseButton.CenterButton => _CurrentMouse.middleButton,
            _ => null,
        };
    }

    /// <summary>
    /// 接続確認
    /// </summary>
    private void CheckConnectMouse()
    {
        var mouse = Mouse.current;
        if (mouse != _CurrentMouse)
        {
            _CurrentMouse = mouse;
            _IsConnectMouse = (_CurrentMouse == null) ? false : true;
        }
    }
}

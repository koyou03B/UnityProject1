using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystemMouse : MonoBehaviour
{
    private Mouse _CurrentMouse;
    private Vector2 _CursorPosition;
    private bool _IsConnectMouse;

    public Vector2 CursorPosition { get { return _CursorPosition; } private set { _CursorPosition = value; } }

    /// <summary>
    /// 初期化
    /// </summary>
    public void OnMouseInit()
    {
        _CurrentMouse = Mouse.current;
        _CursorPosition = Vector2.zero;
        _IsConnectMouse = (_CurrentMouse == null) ? false : true;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined; //これワンちゃんいらない明日見る
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
    public bool GetKeyDown(InputSystemKeyCode.eInputMouseButton key)
    {
        if (!_IsConnectMouse) return false;

        var targetButton = key switch
        {
            InputSystemKeyCode.eInputMouseButton.LeftButton => _CurrentMouse.leftButton,
            InputSystemKeyCode.eInputMouseButton.RightButton => _CurrentMouse.rightButton,
            _ => _CurrentMouse.middleButton,
        };
        return targetButton.wasPressedThisFrame;
    }

    /// <summary>
    /// マウスクリックが離されたとき
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool GetMouseUp(InputSystemKeyCode.eInputMouseButton key)
    {
        if (!_IsConnectMouse) return false;

        var targetButton = key switch
        {
            InputSystemKeyCode.eInputMouseButton.LeftButton => _CurrentMouse.leftButton,
            InputSystemKeyCode.eInputMouseButton.RightButton => _CurrentMouse.rightButton,
            _ => _CurrentMouse.middleButton,
        }; return targetButton.wasReleasedThisFrame;
    }


    /// <summary>
    /// マウスクリックが押されているかどうか
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool GetMousePress(InputSystemKeyCode.eInputMouseButton key)
    {
        if (!_IsConnectMouse) return false;

        var targetButton = key switch
        {
            InputSystemKeyCode.eInputMouseButton.LeftButton => _CurrentMouse.leftButton,
            InputSystemKeyCode.eInputMouseButton.RightButton => _CurrentMouse.rightButton,
            _ => _CurrentMouse.middleButton,
        }; return targetButton.isPressed;
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

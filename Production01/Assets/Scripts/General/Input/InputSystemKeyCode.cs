using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public  class InputSystemKeyCode
{
    public enum eInputMouseButton
    {
        LeftButton,
        RightButton,
        CenterButton,
    }

    public enum eInputKeyType
    {
        Game,
        UI,
    }

    public enum eInputSystemKeyCode
    {
        A,
        B,
        C,
        D,
        E,
        F,
        G,
        H,
        I,
        J,
        K,
        L,
        M,
        N,
        O,
        P,
        Q,
        R,
        S,
        T,
        U,
        V,
        W,
        X,
        Y,
        Z,

        Alpha0,
        Alpha1,
        Alpha2,
        Alpha3,
        Alpha4,
        Alpha5,
        Alpha6,
        Alpha7,
        Alpha8,
        Alpha9,

        Backspace,
        Delete,
        Tab,
        Return,
        Escape,
        Space,
        RightShift,
        LeftShift,
        LeftAlt,
        LeftControl,
        RightControl,

        UpArrow,
        DownArrow,
        RightArrow,
        LeftArrow,

        MoveUp,
        MoveDown,
        MoveRight,
        MoveLeft
    }

    public static Dictionary<eInputSystemKeyCode, Key> DefaultKeyCode { get { return s_dDefaultKeyCode; } private set { s_dDefaultKeyCode = value; } }

    private static Dictionary<eInputSystemKeyCode, Key> s_dDefaultKeyCode = new Dictionary<eInputSystemKeyCode, Key>()
    {
        {eInputSystemKeyCode.A, Key.A },
        {eInputSystemKeyCode.B, Key.B },
        {eInputSystemKeyCode.C, Key.C },
        {eInputSystemKeyCode.D, Key.D },
        {eInputSystemKeyCode.E,  Key.E },
        {eInputSystemKeyCode.F, Key.F },
        {eInputSystemKeyCode.G, Key.G },
        {eInputSystemKeyCode.H, Key.H },
        {eInputSystemKeyCode.I, Key.I },
        {eInputSystemKeyCode.J , Key.J },
        {eInputSystemKeyCode.K,  Key.K },
        {eInputSystemKeyCode.L, Key.L },
        {eInputSystemKeyCode.M, Key.M },
        {eInputSystemKeyCode.N, Key.N },
        {eInputSystemKeyCode.O, Key.O },
        {eInputSystemKeyCode.P, Key.P },
        {eInputSystemKeyCode.Q, Key.Q },
        {eInputSystemKeyCode.R, Key.R },
        {eInputSystemKeyCode.S, Key.S },
        {eInputSystemKeyCode.T, Key.T },
        {eInputSystemKeyCode.U, Key.U },
        {eInputSystemKeyCode.V, Key.V },
        {eInputSystemKeyCode.W, Key.W },
        {eInputSystemKeyCode.X, Key.X },
        {eInputSystemKeyCode.Y, Key.Y },
        {eInputSystemKeyCode.Z, Key.Z },

        {eInputSystemKeyCode.Alpha0, Key.Digit0 },
        {eInputSystemKeyCode.Alpha1, Key.Digit1 },
        {eInputSystemKeyCode.Alpha2, Key.Digit2 },
        {eInputSystemKeyCode.Alpha3, Key.Digit3 },
        {eInputSystemKeyCode.Alpha4, Key.Digit4 },
        {eInputSystemKeyCode.Alpha5, Key.Digit5 },
        {eInputSystemKeyCode.Alpha6, Key.Digit6 },
        {eInputSystemKeyCode.Alpha7, Key.Digit7 },
        {eInputSystemKeyCode.Alpha8, Key.Digit8 },
        {eInputSystemKeyCode.Alpha9, Key.Digit9 },

        { eInputSystemKeyCode.Backspace,    Key.Backspace},
        { eInputSystemKeyCode.Delete,           Key.Delete},
        { eInputSystemKeyCode.Tab,                  Key.Tab},
        { eInputSystemKeyCode.Return,           Key.Enter},
        { eInputSystemKeyCode.Escape,           Key.Escape},
        { eInputSystemKeyCode.Space,            Key.Space},
        { eInputSystemKeyCode.RightShift,       Key.RightShift},
        { eInputSystemKeyCode.LeftShift,        Key.LeftShift},
        { eInputSystemKeyCode.LeftAlt,          Key.LeftAlt},
        { eInputSystemKeyCode.LeftControl,      Key.LeftCtrl},
        { eInputSystemKeyCode.RightControl,     Key.RightCtrl},

        { eInputSystemKeyCode.UpArrow,      Key.UpArrow},
        { eInputSystemKeyCode.DownArrow,    Key.DownArrow},
        { eInputSystemKeyCode.RightArrow,   Key.RightArrow},
        { eInputSystemKeyCode.LeftArrow,        Key.LeftArrow},
    };

}

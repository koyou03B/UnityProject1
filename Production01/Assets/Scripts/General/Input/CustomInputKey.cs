using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using static InputSystemKeyCode;

public class CustomInputKey
{
    //private Dictionary<eInputSystemKeyCode, Key> _dCustomKeyCode;
    private Dictionary<eInputSystemKeyCode, Key> _dActionToKey = new();
    private Dictionary<Key, eInputSystemKeyCode> _dKeyToAction = new();

    private ILogger _Logger;
    private bool _IsLock;

    public CustomInputKey(bool isLock)
    {
        this._IsLock = isLock;
        this._Logger = new PrefixLogger(new UnityLogger(), "[CustomInputKey]");
        _dActionToKey = new Dictionary<eInputSystemKeyCode, Key>()
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

        foreach (var pair in _dActionToKey)
        {
            _dKeyToAction.Add(pair.Value, pair.Key);
        }
    }

    public Key GetKey(eInputSystemKeyCode keyCode) => _dActionToKey[keyCode];

    /// <summary>
    /// 指定したキーコードに反応するキーを変更する
    /// </summary>
    /// <param name="newKey"></param>
    /// <param name="targetAction"></param>
    public void ChangeKeyBinding(Key newKey, eInputSystemKeyCode targetAction)
    {
        if (_IsLock)
        {
            _Logger.LogWarning("CustomInputKey is locked.");
            return;
        }

        // 古いキーを取得
        if (!_dActionToKey.TryGetValue(targetAction, out var oldKey))
        {
            _Logger.LogWarning($"Action {targetAction} not found in bindings.");
            return;
        }

        // 新しいキーに別のアクションがあれば取得
        if (_dKeyToAction.TryGetValue(newKey, out var otherAction))
        {
            // スワップ
            _dKeyToAction[newKey] = targetAction;
            _dActionToKey[targetAction] = newKey;

            _dKeyToAction[oldKey] = otherAction;
            _dActionToKey[otherAction] = oldKey;

            _Logger.Log($"Swapped: {targetAction}⇔{otherAction} (keys {oldKey}⇔{newKey})");
        }
        else
        {
            // 単純な移動
            _dKeyToAction.Remove(oldKey);
            _dKeyToAction[newKey] = targetAction;
            _dActionToKey[targetAction] = newKey;

            _Logger.Log($"Changed: {targetAction} moved from {oldKey} to {newKey}");
        }
    }

    /// <summary>
    /// キーコードの保存するパックの作成
    /// </summary>
    /// <returns></returns>
    public List<int> CreateSavePackKeyCode()
    {
        var result = new List<int>();
        var count = _dActionToKey.Count;

        //2つずつ処理
        for (int i = 0; i < count; i += 2)
        {
            var keyPair1 = _dActionToKey.ElementAt(i);
            byte key1 = (byte)keyPair1.Key;
            byte value1 = (byte)keyPair1.Value;

            byte key2 = 0, value2 = 0;
            if (i + 1 < count)
            {
                var keyPair2 = _dActionToKey.ElementAt(i + 1); //次のペア(ない可能性もある)
                key2 = (byte)keyPair2.Key;
                value2 = (byte)keyPair2.Value;
            }

            // 2つのペアを1つのintにパック
            int packed = (key1 << 24) | (value1 << 16) | (key2 << 8) | value2;
            result.Add(packed);
        }

        _Logger.Log("Completed CreateSavePackKeyCode ");
        return result;
    }

    /// <summary>
    /// キーコードパックを収める
    /// </summary>
    /// <param name="packedData"></param>
    public void SetKeyCodePack(List<int> packedData)
    {
        foreach (int packed in packedData)
        {
            // packedから値を取り出す
            byte key1 = (byte)((packed >> 24) & 0xFF); // 最上位8bit
            byte value1 = (byte)((packed >> 16) & 0xFF); // 次の8bit
            byte key2 = (byte)((packed >> 8) & 0xFF);  // 次の8bit
            byte value2 = (byte)(packed & 0xFF);          // 最下位8bit

            _dActionToKey[(eInputSystemKeyCode)key1] = (Key)value1;
            _dKeyToAction[(Key)value1] =(eInputSystemKeyCode)key1;
            //0と0の可能性もあるので確認
            if (key2 != 0 && value2 != 0)
            {
                _dActionToKey[(eInputSystemKeyCode)key2] = (Key)value2;
                _dKeyToAction[(Key)value2] = (eInputSystemKeyCode)key2;
            }
        }
        _Logger.Log("Completed SetKeyCodePack ");
    }
}

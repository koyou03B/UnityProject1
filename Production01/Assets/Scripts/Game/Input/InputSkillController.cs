using UnityEngine;

public delegate void OnSkillInputDetected(int index,SkillEnums.eSkillSlotType eSkillSlot);
public class InputSkillController
{
    private InputSystemKeyCode.eInputSystemKeyCode[] _MainSkillKeys;
    private InputSystemKeyCode.eInputSystemKeyCode[] _SubSkillKeys;

    private OnSkillInputDetected _OnSkillInputDetected;

    /// <summary>
    /// Mapperから頂く情報で設定する
    /// </summary>
    /// <param name="index"></param>
    /// <param name="keyCode"></param>
    /// <param name="eSkillSlotType"></param>
    public void SetSkillKey(int index, InputSystemKeyCode.eInputSystemKeyCode keyCode,SkillEnums.eSkillSlotType eSkillSlotType)
    {
        var skillKeys = eSkillSlotType == SkillEnums.eSkillSlotType.Main ? _MainSkillKeys : _SubSkillKeys;
        skillKeys[index] = keyCode;
    }

    public InputSkillController()
    {
        _MainSkillKeys = new InputSystemKeyCode.eInputSystemKeyCode[CharacterSkillContext.MainSkillCount];
        _SubSkillKeys = new InputSystemKeyCode.eInputSystemKeyCode[CharacterSkillContext.SubSkillCount];
        //いれる関数を作成してからいれる
        //  OnSkillInputDetected+=
    }

    /// <summary>
    /// 毎フレーム処理
    /// </summary>
    public void Poling()
    {
        bool ignition = false;
        var keyboard = InputSystemController.Instance;
        for (int i = 0; i < _MainSkillKeys.Length;i++)
        {
            //InputSystemControllerの方でLockがかかってたら動かないようになってる
            if (keyboard.GetKeyDown(_MainSkillKeys[i]))
            {
                _OnSkillInputDetected?.Invoke(i,SkillEnums.eSkillSlotType.Main);
                ignition = true;
                break;
            }
        }
        //すでに発火したなら止める
        if (ignition) return;

        for (int i = 0; i < _SubSkillKeys.Length; i++)
        {
            //InputSystemControllerの方でLockがかかってたら動かないようになってる
            if (keyboard.GetKeyDown(_SubSkillKeys[i]))
            {
                _OnSkillInputDetected?.Invoke(i, SkillEnums.eSkillSlotType.Sub);
                break;
            }
        }

        //アイテムスキルとかを増やすならここに
    }
}

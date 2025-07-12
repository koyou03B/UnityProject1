using System;
using System.ComponentModel;
using UnityEngine;

public class SkillEnums
{
    public enum SkillTriggerTiming
    {
        None,
        KeyDown,            //押したとき
        KeyRelease,         //キーを離した瞬間
        TimedTrigger       //タイマーや一定条件による自動発動
    }

    //スロットにあてるスキルタイプ
    public enum eSkillSlotType
    {
        None,
        Main,
        Sub,
    }
    //個人的にわかんなくなる時が来るから調整する
    [Flags]
    public enum SkillCategory
    {
        [Description("何もないです")]
        None,
        [Description("ダメージを与えるスキル")]
        Offensive = 1 << 0,
        [Description("ダメージを防ぐスキル")]
        Defensive = 1 << 1,
        [Description("味方を強化・回復するスキル")]
        Support = 1 << 2,
        [Description("移動・位置変更系スキル")]
        Mobility = 1 << 3,
        [Description("相手の行動を制限するスキル")]
        Control = 1 << 4
    }
}

using UnityEngine;

public static class SaveLoadEnum
{
    public enum eSaveType
    {
        AllData,
        System,
        Input,  //入力 これシステム統合
        Slot1,
        Slot2,
        Slot3,
        //ここから各ゲームごとの保存したい名称たち
        //例えばSkillとか
        Skill,  //スキル
    }

}

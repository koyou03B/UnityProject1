using UnityEngine;

public static class SaveLoadEnum
{
    public enum eSaveType
    {
        AllData,
        System,
        //ここから各ゲームごとの保存したい名称たち
        //例えばSkillとか
        Input,  //入力 これシステム統合
        Skill,  //スキル
    }

}

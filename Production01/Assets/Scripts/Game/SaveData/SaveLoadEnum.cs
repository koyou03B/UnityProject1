using UnityEngine;

public static class SaveLoadEnum
{
    public enum eSaveErrorType
    {
        ShortageFs,   // ファイル作成不足
        NoSpaceFs,    // 容量不足
        Coprupt,      // 壊れている
        UunkownError, // 不明なエラー
    }


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

    public enum eSaveLoadAction
    {
        None,
        Save,
        Load,
        Deleate,
        Error,
    }
}

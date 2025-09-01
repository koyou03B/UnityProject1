using UnityEngine;

public partial class SaveLoadTags
{
    public enum eTopTag : byte
    {
        General,
        Slot1,
        Slot2,
        Slot3,
    }

    public enum eInnerTypeTag : byte
    {
        Input,      //入力内容
        Option,     //オプション
        GameSystem, //ゲーム特有のシステム 

        Skill,
    }
}

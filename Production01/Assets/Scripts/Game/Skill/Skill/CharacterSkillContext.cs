using UnityEngine;

public class CharacterSkillContext
{
    public static readonly byte MainSkillCount = 3;
    public static readonly byte SubSkillCount = 2;

    private SkillSlot[] _MainSkillSlots;
    private SkillSlot[] _SubSkillSlots;

    public CharacterSkillContext()
    {
        _MainSkillSlots = new SkillSlot[MainSkillCount];
        _SubSkillSlots = new SkillSlot[SubSkillCount];
    }

    #region 与える関数たち
    /// <summary>
    /// 対応するスキルSlotのKeyCodeを渡す
    /// </summary>
    /// <param name="type"></param>
    /// <param name="slot"></param>
    /// <returns></returns>
    public InputSystemKeyCode.eInputSystemKeyCode SkillkeyCode(SkillEnums.eSkillSlotType type,int slot)
    {
        bool sizError = type switch
        {
            SkillEnums.eSkillSlotType.Main => slot < 0 && slot >= MainSkillCount,
            SkillEnums.eSkillSlotType.Sub => slot < 0 && slot >= SubSkillCount,
            _ => true
        };

        if (sizError) return InputSystemKeyCode.eInputSystemKeyCode.None;

        return type == SkillEnums.eSkillSlotType.Main ? _MainSkillSlots[slot].eInputSystemKeyCode : _SubSkillSlots[slot].eInputSystemKeyCode;
    }

       /// <summary>
       /// 該当するSkillDataを渡す
       /// 上記の関数からの流れで来るのでエラーになることがない流れ
       /// </summary>
       /// <param name="type"></param>
       /// <param name="slot"></param>
       /// <returns></returns>
    public SkillDefinition.SkillData SkillDataBySkillSlot(SkillEnums.eSkillSlotType type, int slot)
    {
        SkillSlot skillSlot = type == SkillEnums.eSkillSlotType.Main ? _MainSkillSlots[slot] : _SubSkillSlots[slot];
        SkillDefinition skillDefinition = GlobalAssetController.Instance.SkillDefinition(skillSlot.SkillReference.eMainSkillCategory);

        return skillDefinition.FindTargetSkillData(skillSlot.SkillReference.SkillTier, skillSlot.SkillReference.SkillID);
    }
    #endregion

    #region こっちで処理するモノ
    /// <summary>
    /// 初期設定
    /// </summary>
    /// <param name="mainSkills"></param>
    /// <param name="subSkills"></param>
    public void OnInitialize(ref SkillSlot[] mainSkills, ref SkillSlot[] subSkills)
    {
        for (int i = 0; i < MainSkillCount; i++)
        {
            _MainSkillSlots[i] = new SkillSlot();
            _MainSkillSlots[i] = mainSkills[i];
        }
        for (int i = 0; i < SubSkillCount; i++)
        {
            _SubSkillSlots[i] = new SkillSlot();
            _SubSkillSlots[i] = subSkills[i];
        }
    }

    public SkillSlot[] MainSkillSlotAll { get { return _MainSkillSlots; } }
    public SkillSlot[] SubSkillSlotAll { get { return _SubSkillSlots; } }
    #endregion
}

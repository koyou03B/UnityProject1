using UnityEngine;

public struct SkillSlot
{
    //対応するキー
    private InputSystemKeyCode.eInputSystemKeyCode _eInputSystemKeyCode;
    //リファレンス(探すための材料)
    private SkillReference _SkillReference;

    public SkillSlot(InputSystemKeyCode.eInputSystemKeyCode keyCode, SkillEnums.SkillCategory category, uint tier, uint id)
    {
        this._eInputSystemKeyCode = keyCode;
        this._SkillReference = new SkillReference(category, tier, id);
    }
    public InputSystemKeyCode.eInputSystemKeyCode eInputSystemKeyCode => _eInputSystemKeyCode;
    public SkillReference SkillReference => _SkillReference;

    public bool HasAssignedSkill => SkillReference.SkillID > 0;
    public void ChangeKeyCode(InputSystemKeyCode.eInputSystemKeyCode newKeyCode) => _eInputSystemKeyCode = newKeyCode;
    public void ChangeSkillReference(SkillReference newSkillReference) => _SkillReference = newSkillReference;

   
}


public struct SkillReference
{
    //対応しているスキルメインカテゴリー
    private SkillEnums.SkillCategory _eMainSkillCategory;
    //スキルティア
    private uint _SkillTier;
    //スキルID
    private uint _SkillID;

    public SkillReference(SkillEnums.SkillCategory category, uint tier,uint id)
    {
        this._eMainSkillCategory = category;
        this._SkillTier = tier;
        this._SkillID = id;
    }

    //対応しているスキルメインカテゴリー
    public SkillEnums.SkillCategory eMainSkillCategory => _eMainSkillCategory;
    //スキルティア
    public uint SkillTier => _SkillTier;
    //スキルID
    public uint SkillID => _SkillID;
}

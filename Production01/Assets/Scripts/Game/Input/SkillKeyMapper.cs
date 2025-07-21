using UnityEngine;

public class SkillKeyMapper
{
    private readonly CharacterSkillContext _CharacterSkill;
    private readonly InputSkillController _InputSkillCtrl;
    private PrefixLogger _Logger;
    public SkillKeyMapper(InputSkillController inputSkillCtrl, CharacterSkillContext characterSkill)
    {
        _InputSkillCtrl = inputSkillCtrl;
        _CharacterSkill = characterSkill;
        _Logger = new PrefixLogger(new UnityLogger(), "[SkillKeyMapper");
    }

    /// <summary>
    /// すべてのSlotのKeyを設定する
    /// </summary>
    public void AllMappingSlotByKey()
    {
        var skillSlot = _CharacterSkill.MainSkillSlotAll;
        for (int i = 0; i < CharacterSkillContext.MainSkillCount; i++)
        {
            //スキルが設定されていれば
            if (skillSlot[i].HasAssignedSkill())
            {
                _InputSkillCtrl.SetSkillKey(i, skillSlot[i].eInputSystemKeyCode, SkillEnums.eSkillSlotType.Main);
            }
            else
            {
                _Logger.LogError($"No key has been set for SkillEnums.eSkillSlotType.Main{i}");
            }
        }

        skillSlot = _CharacterSkill.SubSkillSlotAll;
        for (int i = 0; i < CharacterSkillContext.SubSkillCount; i++)
        {
            //スキルが設定されていれば
            if (skillSlot[i].HasAssignedSkill())
            {
                _InputSkillCtrl.SetSkillKey(i, skillSlot[i].eInputSystemKeyCode, SkillEnums.eSkillSlotType.Sub);
            }
            else
            {
                _Logger.LogError($"No key has been set for SkillEnums.eSkillSlotType.Sub{i}");
            }
        }
    }

    /// <summary>
    /// 指定されたSlotのKeyを新しく設定する
    /// </summary>
    /// <param name="eSkillSlotType"></param>
    /// <param name="index"></param>
    public void TargetMappingSlotByKey(SkillEnums.eSkillSlotType eSkillSlotType, int index)
    {
        var skillSlot = eSkillSlotType == SkillEnums.eSkillSlotType.Main? _CharacterSkill.MainSkillSlotAll : _CharacterSkill.SubSkillSlotAll;
        //スキルが設定されていれば
        if(skillSlot[index].HasAssignedSkill())
        {
            _InputSkillCtrl.SetSkillKey(index, skillSlot[index].eInputSystemKeyCode, eSkillSlotType);
        }
        else
        {
            _Logger.LogWarning($"The key change may not have worked properly:{index},{eSkillSlotType}");
        }
    }
}

using UnityEngine;

public class SkillSlotHandler
{
    private readonly CharacterSkillContext _Context;
    private PrefixLogger _Logger;
    public SkillSlotHandler(CharacterSkillContext context)
    {
        _Context = context;
        _Logger = new PrefixLogger(new UnityLogger(), "SkillSlothandler");
    }

    /// <summary>
    /// スキルのキーだけ変更
    /// </summary>
    /// <param name="type"></param>
    /// <param name="slotIndex"></param>
    /// <param name="newKey"></param>
    public void ChangeKey(SkillEnums.eSkillSlotType type, int slotIndex, InputSystemKeyCode.eInputSystemKeyCode newKey)
    {
        SkillSlot[] skillSlots = FindSkillSlotType(type);

        if (skillSlots == null)
        {
            _Logger.LogError($"eSkillSlotType is None : {type}");
            return;
        }

        for (int i = 0; i < skillSlots.Length; i++)
        {
            //参照渡し
            ref SkillSlot slot = ref skillSlots[i]; 
            if (slot.eInputSystemKeyCode == newKey)
            {
                var oldKey = skillSlots[slotIndex].eInputSystemKeyCode;
                slot.ChangeKeyCode(oldKey);
                break;
            }
        }
        //slot名前被りで許してくれないからスコープで囲む(名前変えればいいだけだけなんだけどね)
        {
            ref SkillSlot slot = ref skillSlots[slotIndex]; 
            skillSlots[slotIndex].ChangeKeyCode(newKey);
        }

    }

    /// <summary>
    /// スキルを変更する
    /// こちらでは同じスキルを入れれないように処理されたのが来る
    /// </summary>
    /// <param name="type"></param>
    /// <param name="slotIndex"></param>
    /// <param name="newSkill"></param>
    public void ChangeSkill(SkillEnums.eSkillSlotType type, int slotIndex, SkillReference newSkill)
    {
        SkillSlot[] skillSlots = FindSkillSlotType(type);

        if (skillSlots == null)
        {
            _Logger.LogError($"eSkillSlotType is None : {type}");
            return;
        }

        ref SkillSlot slot = ref skillSlots[slotIndex];
        slot.ChangeSkillReference(newSkill);
    }


    private SkillSlot[] FindSkillSlotType(SkillEnums.eSkillSlotType type)
    {
        SkillSlot[] skillSlots = type switch
        {
            SkillEnums.eSkillSlotType.Main => _Context.MainSkillSlotAll,
            SkillEnums.eSkillSlotType.Sub => _Context.SubSkillSlotAll,
            _ => null,
        };

        return skillSlots;
    }
}

using UnityEngine;
using System.Collections.Generic;
using System;

public partial class SkillDataPacker : IDataPacker<SkillSlot[]>
{
    private byte[] PackVersion0(SkillSlot[] slots)
    {
        var payload = new List<byte>();
        foreach (var slot in slots)
        {
            BitUtility.WriteInt(payload, (int)slot.eInputSystemKeyCode);
            BitUtility.WriteInt(payload, (int)slot.SkillReference.eMainSkillCategory);
            BitUtility.WriteUInt(payload, slot.SkillReference.SkillTier);
            BitUtility.WriteUInt(payload, slot.SkillReference.SkillID);
        }

        return payload.ToArray();
    }

    private void SetDefaultPackage(byte version, SkillSlot[] data)
    {
        InputSystemKeyCode.eInputSystemKeyCode keyCode = InputSystemKeyCode.eInputSystemKeyCode.None;
        SkillEnums.SkillCategory skillCategory = SkillEnums.SkillCategory.None;
        uint tier = 0, id = 0;

        for (int i = 0; i < CharacterSkillContext.MainSkillCount; i++)
        {
            switch (i)
            {
                case 0:
                    keyCode = InputSystemKeyCode.eInputSystemKeyCode.W;
                    skillCategory = SkillEnums.SkillCategory.Offensive;
                    tier = 0;
                    id = 100;
                    break;
                case 1:
                    keyCode = InputSystemKeyCode.eInputSystemKeyCode.E;
                    skillCategory = SkillEnums.SkillCategory.Offensive;
                    tier = 0;
                    id = 101;
                    break;
                case 2:
                    keyCode = InputSystemKeyCode.eInputSystemKeyCode.R;
                    skillCategory = SkillEnums.SkillCategory.Offensive;
                    tier = 0;
                    id = 102;
                    break;
            }
            data[i] = new SkillSlot(keyCode, skillCategory, tier, id);
        }
        for (int i = 0; i < CharacterSkillContext.SubSkillCount; i++)
        {
            switch (i)
            {
                case 0:
                    keyCode = InputSystemKeyCode.eInputSystemKeyCode.Alpha1;
                    skillCategory = SkillEnums.SkillCategory.Support;
                    tier = 0;
                    id = 300;
                    break;
                case 1:
                    keyCode = InputSystemKeyCode.eInputSystemKeyCode.Alpha2;
                    skillCategory = SkillEnums.SkillCategory.Support;
                    tier = 0;
                    id = 301;
                    break;
            }
            data[CharacterSkillContext.MainSkillCount + i] = new SkillSlot(keyCode, skillCategory, tier, id);
        }
    }

    private bool UnPackVersion0(byte[] payload,SkillSlot[] slots)
    {
        if (payload == null || payload.Length ==0 ||payload.Length % 16 != 0)
        {
            SetDefaultPackage(0, slots);
            return true;
        }

        int count = payload.Length / 16;
        int offset = 0;
        for (int i = 0; i < count; i++)
        {
            var keyCode = (InputSystemKeyCode.eInputSystemKeyCode)BitUtility.ReadInt(payload, ref offset);
            var category = (SkillEnums.SkillCategory)BitUtility.ReadInt(payload, ref offset);
            uint tier = BitUtility.ReadUInt(payload, ref offset);
            uint id = BitUtility.ReadUInt(payload, ref offset); ;

            slots[i] = new SkillSlot(keyCode, category, tier, id);
        }

        return true;
    }


}

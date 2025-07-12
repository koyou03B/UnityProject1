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

    private bool UnPackVersion0(byte[] payload,SkillSlot[] slots)
    {
        if (payload == null || payload.Length % 16 != 0) return false;

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

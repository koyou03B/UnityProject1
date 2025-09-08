using System;
using UnityEngine;

public partial class SkillDataPacker : IDataPacker<SkillSlot[]>
{
    public const byte Version0 = 0;
    public const int SlotCountV0 = 5;

    private ILogger _Logger = new PrefixLogger(new UnityLogger(), "[SkillDataPacker]");
    public byte[] PackPayload(SkillSlot[] slots, byte version)
    {
        switch(version)
        {
            case Version0:
                return PackVersion0(slots);
        }
        _Logger.LogError($"Unsupported version: {version}");
        return new byte[0];
    }

    public bool TryUnpackPayload(ReadOnlySpan<byte> payload, byte version, out SkillSlot[] slots)
    {
        switch (version)
        {
            case Version0:
                slots = new SkillSlot[SlotCountV0];
                return UnPackVersion0(payload.ToArray(), slots);
        }
        slots = new SkillSlot[CharacterSkillContext.MainSkillCount + CharacterSkillContext.SubSkillCount];
        _Logger.LogError($"Unsupported version: {version}");
        return false;
    }
}

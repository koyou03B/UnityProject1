using UnityEngine;

public sealed partial class GlobalReadOnlySaveData
{
    private SkillSlot[] _SkillSlots = new SkillSlot[SkillDataPacker.SlotCountV0];
    private SkillDataPacker _SkillDataPacker = new SkillDataPacker();

    public SkillSlot[] SkillSlots { get { return _SkillSlots; } }
    public SkillSlot GetSkillSlot(int index)
    {
        if(index >= _SkillSlots.Length)
        {
            _Logger.LogWarning($"{index} is sizeOver for {_SkillSlots}");
            index = 0;
        }

        return _SkillSlots[index];
    }

    /// <summary>
    /// 読み取り専用のデータを作成
    /// </summary>
    /// <param name="raw"></param>
    private void ParseSkillData(byte[] rawSkillSlotData)
    {
        if (!BytePacker.TryUnpack(rawSkillSlotData, out byte type, out byte version, out byte[] payload))
        {
            _Logger.LogError($"{rawSkillSlotData} Failed to unpack input data.");
        }

        if (type != (byte)SaveTypeEnum.eSaveCategory.Skill)
        {
            _Logger.LogError($"Unexpected type: {type}");
        }

        if (!_SkillDataPacker.TryUnpackPayload(payload, version, out _SkillSlots))
        {
            _Logger.LogError($"this payload{rawSkillSlotData} is broken");
        }
    }
}

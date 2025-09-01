using UnityEngine;
using System.Collections.Generic;

public class SkillSaveLoader 
{
    private readonly byte _Version = 0;
    private CharacterSkillContext _SkillContext;
    private SkillDataPacker _SkillDataPacker;
    private ILogger _Logger;

    public SkillSaveLoader(CharacterSkillContext skillContext)
    {
        this._SkillContext = skillContext;
        _SkillDataPacker = new SkillDataPacker();
        _Logger = new PrefixLogger(new UnityLogger(), "[SkillSaveLoader]");

    }

    public void SaveAllSkillData()
    {
        SkillSlot[] skillSlots = new SkillSlot[CharacterSkillContext.MainSkillCount + CharacterSkillContext.SubSkillCount];
        SkillSlot[] mainSkills = _SkillContext.MainSkillSlotAll;
        for(int i =0; i < mainSkills.Length;i++)
        {
            skillSlots[i] = mainSkills[i];
        }
        SkillSlot[] subSkills = _SkillContext.SubSkillSlotAll;
        for(int i =0; i < subSkills.Length;i++)
        {
            skillSlots[CharacterSkillContext.MainSkillCount  + i] = subSkills[i];
        }

        byte[] payload = _SkillDataPacker.PackPayload(skillSlots, _Version);

        var packData =  BytePacker.Pack((int)SaveLoadTags.eInnerTypeTag.Skill, 0, payload);
        //Rawに保存
        GlobalRawSaveData.Instance.SetRawSkillData(packData);
    }

    /// <summary>
    /// Loadしたデータを当てはめてもらう
    /// </summary>
    /// <param name="version"></param>
    /// <param name="rawData"></param>
    public void LoadAllSkillSlotPack(byte[] packedData)
    {
        if (!BytePacker.TryUnpack(packedData, out byte type, out byte version, out byte[] payload))
        {
            _Logger.LogError("Failed to unpack input data.");
        }

        if (type != (byte)SaveLoadTags.eInnerTypeTag.Skill)
        {
            _Logger.LogError($"Unexpected type: {type}");
        }

        if(!_SkillDataPacker.TryUnpackPayload(payload,version,out SkillSlot[] skillSlots))
        {
            _Logger.LogError("this payload is broken");
        }

        SetSkillSlotPack(skillSlots);
    }


    /// <summary>
    /// version0のスキルセット(0しかないと思うが)
    /// </summary>
    /// <param name="skillPack"></param>
    private void SetSkillSlotPack(SkillSlot[] skillSlots)
    {
        SkillSlot[] mainSkillSlot = new SkillSlot[CharacterSkillContext.MainSkillCount];
        SkillSlot[] subSkillSlot = new SkillSlot[CharacterSkillContext.SubSkillCount];

        for (int i = 0; i < mainSkillSlot.Length; i++)
        {
            mainSkillSlot[i] = skillSlots[i];
        }
        for (int i = 0; i < subSkillSlot.Length; i++)
        {
            subSkillSlot[i] = skillSlots[CharacterSkillContext.MainSkillCount + i];
        }
        _SkillContext.OnInitialize(ref mainSkillSlot, ref  subSkillSlot);
    }
}

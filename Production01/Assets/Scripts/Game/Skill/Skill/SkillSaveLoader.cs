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

        var packData =  BytePacker.Pack((int)SaveTypeEnum.eSaveCategory.Skill, 0, payload);
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
        if(packedData.Length == 0)
        {
            CreateDefaultSkillSlot();
            return;
        }

        if (!BytePacker.TryUnpack(packedData, out byte type, out byte version, out byte[] payload))
        {
            _Logger.LogError("Failed to unpack input data.");
        }

        if (type != (byte)SaveTypeEnum.eSaveCategory.Skill)
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

    /// <summary>
    /// デフォルトでセット
    /// </summary>
    private void CreateDefaultSkillSlot()
    {
        SkillSlot[] mainSkillSlot = new SkillSlot[CharacterSkillContext.MainSkillCount];
        SkillSlot[] subSkillSlot = new SkillSlot[CharacterSkillContext.SubSkillCount];
        InputSystemKeyCode.eInputSystemKeyCode keyCode = InputSystemKeyCode.eInputSystemKeyCode.None;
        SkillEnums.SkillCategory skillCategory = SkillEnums.SkillCategory.None;
        uint tier = 0, id = 0;

        for (int i = 0; i < CharacterSkillContext.MainSkillCount;i++)
        {
            switch(i)
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
            mainSkillSlot[i] = new SkillSlot(keyCode, skillCategory, tier, id);
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
            subSkillSlot[i] = new SkillSlot(keyCode, skillCategory, tier, id);
        }

        _SkillContext.OnInitialize(ref mainSkillSlot, ref subSkillSlot);
    }
}

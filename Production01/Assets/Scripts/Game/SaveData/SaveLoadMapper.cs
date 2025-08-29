using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Hook:特定の処理の前後や途中に自分の処理を差し込む意味
/// </summary>
public class SaveLoadHooks
{
    private readonly GlobalRawSaveData.RawDataMapping _OnBeforeSaveAction;//ファイルにセーブする前に
    private readonly Action<byte[],bool> _OnBeforeDeserialize;//GlobalRawに飛ばすよう
    private readonly Action<byte[]>　_OnAfterLoad;//GlobalReadOnlyに飛ばす用

    public SaveLoadHooks(GlobalRawSaveData.RawDataMapping onBeforSaveAction, Action<byte[],bool> onBeforeDeserialize, Action<byte[]> onAfterLoad)
    {
        _OnBeforeSaveAction = onBeforSaveAction;
        _OnBeforeDeserialize = onBeforeDeserialize;
        _OnAfterLoad = onAfterLoad;
    }

    /// <summary>
    /// Rawデータからセーブする配列を頂く
    /// </summary>
    public byte[] GetRawSaveData => _OnBeforeSaveAction?.Invoke();
    
    /// <summary>
    /// ロードしたデータをゲーム側に渡す
    /// </summary>
    /// <param name="loadData"></param>
    public void SetGameState(byte[] loadData)
    {
        _OnBeforeDeserialize?.Invoke(loadData,false);
        _OnAfterLoad?.Invoke(loadData);
    }
}

public class SaveLoadMapper : MonoBehaviour
{
    private Dictionary<SaveLoadEnum.eSaveType, SaveLoadHooks> _dMappingActions;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Setup()
    {
        var globalRaw = GlobalRawSaveData.Instance;
        var globalReadOnly = GlobalReadOnlySaveData.Instance;
        _dMappingActions = new Dictionary<SaveLoadEnum.eSaveType, SaveLoadHooks>()
        {
            { SaveLoadEnum.eSaveType.System,new SaveLoadHooks(globalRaw.OnRawSystemDataMapping,globalRaw.SetRawSystemData,globalReadOnly.ParseSystemData)},
            { SaveLoadEnum.eSaveType.Input,new SaveLoadHooks(globalRaw.OnRawInputDataMapping,globalRaw.SetRawInputData,globalReadOnly.ParseInputData)},
            { SaveLoadEnum.eSaveType.Skill,new SaveLoadHooks(globalRaw.OnRawSkillDataMapping,globalRaw.SetRawSkillData,globalReadOnly.ParseSkillData)},
        };
    }

    /// <summary>
    /// 指定されたRawDataを渡す
    /// </summary>
    /// <param name="saveType"></param>
    /// <returns></returns>
    public byte[] FindRawSaveData(SaveLoadEnum.eSaveType saveType)
    {
        bool isGet = _dMappingActions.TryGetValue(saveType, out SaveLoadHooks hooks);
        if (isGet)
        {
            return hooks.GetRawSaveData;
        }
        //見つからない場合はそれ用を新しく作る
        byte[] payload = new byte[1];
        return BytePacker.Pack((byte)saveType, 0, payload);
    }

    /// <summary>
    /// 受け取ったデータを渡す
    /// </summary>
    /// <param name="data"></param>
    /// <param name="saveType"></param>
    public void DeserializeSaveLoadData(byte[] data, SaveLoadEnum.eSaveType saveType)
    {
        _dMappingActions[saveType].SetGameState(data);
    }
}

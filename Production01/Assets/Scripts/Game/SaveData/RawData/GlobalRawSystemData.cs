using UnityEditor;
using UnityEngine;

public sealed partial class GlobalRawSaveData
{
    private byte[] _RawSystemData;
    private RawDataMapping _OnRawSystemDataMapping;
    /// <summary>
    /// 外部で書き換えを禁じます
    /// </summary>
    public byte[] RawSystemData { get { return _RawSystemData; } }

    public RawDataMapping OnRawSystemDataMapping => _OnRawSystemDataMapping;

    /// <summary>
    /// データの存在確認
    /// </summary>
    public bool HasSystemData => _RawSystemData != null && _RawSystemData.Length > 0;

    /// <summary>
    /// 新しいデータに書き換える
    /// </summary>
    /// <param name="newData"></param>
    public void SetRawSystemData(byte[] newData,bool updateSaveType = true)
    {
        if (_RawSystemData == null || _RawSystemData.Length != newData.Length)
        {
            _RawSystemData = new byte[newData.Length];
        }

        _RawSystemData = newData;
        if(updateSaveType)
        {
            _UpdateSaveTypeList.Add(SaveLoadEnum.eSaveType.System);
        }
    }
    private void SetupRawSystemData()
    {
        _OnRawSystemDataMapping = new RawDataMapping(() => 
        {
            if (_RawSystemData == null || _RawSystemData.Length != 0)
            {
                //見つからない場合はそれ用を新しく作る
                byte[] payload = new byte[1];
                return BytePacker.Pack((byte)SaveLoadEnum.eSaveType.System, 0, payload);
            }

            return _RawSkillData;
        }
        
        );
    }
}

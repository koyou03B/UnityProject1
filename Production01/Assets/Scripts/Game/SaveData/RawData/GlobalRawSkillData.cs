using UnityEngine;

/// <summary>
/// SkillDataを纏めておく
/// </summary>
public  sealed partial class GlobalRawSaveData
{
    private byte[] _RawSkillData;

    private RawDataMapping _OnRawSkillDataMapping;

    /// <summary>
    /// 外部で書き換えを禁じます
    /// </summary>
    public byte[] RawSkillData { get { return _RawSkillData; } }

    /// <summary>
    /// Saveファイルに送る用
    /// </summary>
    public RawDataMapping OnRawSkillDataMapping => _OnRawSkillDataMapping;

    /// <summary>
    /// データの存在確認
    /// </summary>
    public bool HasSkillData => _RawSkillData != null && _RawSkillData.Length > 0;

    /// <summary>
    /// 新しいデータに書き換える
    /// </summary>
    /// <param name="newData"></param>
    public void SetRawSkillData(byte[] newData)
    {
        if (newData == null)
        {
            _RawSkillData = new byte[newData.Length];
        }
        //配列は参照型なのでクローンする
        _RawSkillData = (byte[])newData.Clone();
    }

    /// <summary>
    /// 各々のセットアップ
    /// </summary>
    private void SetupSkillData()
    {
        _OnRawSkillDataMapping = new RawDataMapping(() => _RawSkillData);
    }
}

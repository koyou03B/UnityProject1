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
    public void SetRawSystemData(byte[] newData)
    {
        if (newData == null)
        {
            _RawSystemData = new byte[newData.Length];
        }
        //配列は参照型なのでクローンする
        _RawSystemData = (byte[])newData.Clone();
    }
    private void SetupRawSystemData()
    {
        _OnRawSystemDataMapping = new RawDataMapping(() => _RawSystemData);
    }
}

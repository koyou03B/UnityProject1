using UnityEngine;

public sealed partial class GlobalRawSaveData
{
    private byte[] _RawInputData;

    private RawDataMapping _OnRawInputDataMapping;

    /// <summary>
    /// 外部で書き換えを禁じます
    /// </summary>
    public byte[] RawInputData { get { return _RawInputData; } }

    /// <summary>
    /// Saveファイルに送る用
    /// </summary>
    public RawDataMapping OnRawInputDataMapping => _OnRawInputDataMapping;

    /// <summary>
    /// データの存在確認
    /// </summary>
    public bool HasInputData => _RawInputData != null && _RawInputData.Length > 0;

    /// <summary>
    /// 新しいデータに書き換える
    /// </summary>
    /// <param name="newData"></param>
    public void SetRawInputData(byte[] newData)
    {
        if (newData == null)
        {
            _RawInputData = new byte[newData.Length];
        }
        //配列は参照型なのでクローンする
        _RawInputData = (byte[])newData.Clone();
    }

    /// <summary>
    /// 各々のセットアップ
    /// </summary>
    private void SetupInputData()
    {
        _OnRawInputDataMapping = new RawDataMapping(() => _RawInputData);
    }
}

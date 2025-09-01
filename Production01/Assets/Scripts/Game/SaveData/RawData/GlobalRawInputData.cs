using System;
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
    public void SetRawInputData(byte[] newData,bool updateSaveType = true)
    {
        if (_RawInputData == null || _RawInputData.Length != newData.Length)
        {
            _RawInputData = new byte[newData.Length];
        }
        //配列は参照型なのでクローンする
        _RawInputData = new ArraySegment<byte>(newData, 0, newData.Length).ToArray();
        if (updateSaveType)
        {
            _UpdateSaveTypeList.Add(SaveLoadTags.eInnerTypeTag.Input);
        }
    }

    /// <summary>
    /// 各々のセットアップ
    /// </summary>
    private void SetupInputData()
    {
        _OnRawInputDataMapping = new RawDataMapping(() =>
        {
            if (_RawInputData == null || _RawInputData.Length == 0)
            {
                //見つからない場合はそれ用を新しく作る
                return BytePacker.Pack((byte)SaveLoadTags.eInnerTypeTag.Input, 0, Array.Empty<byte>());
            }

            return _RawInputData;
        }

        );
    }
}

using UnityEngine;
using System;
using System.Collections.Generic;
using NUnit.Framework.Constraints;

public partial class SaveLoadBuffer : MonoBehaviour
{
    private SaveLoadMapper _SaveLoadMapper;
    private PrefixLogger _Logger;
    //「変更があった部分」かつ「適用対象部分」として保存
    private List<SaveLoadEnum.eSaveType> _AffectedSaveTypes;//affected:影響を受けた
    private byte[] _SaveLoadData;

    /// <summary>
    /// 保存しているデータを読み取り用として渡す（同期処理用）
    /// </summary>
    public ReadOnlySpan<byte> GetSaveLoadDataSpan() => new ReadOnlySpan<byte>(_SaveLoadData);
 
    /// <summary>
    /// 保存しているデータをそのまま参照で渡す（非同期処理用）
    /// </summary>
    public byte[] GetSaveLoadDataArray() => _SaveLoadData;

    private void Awake()
    {
        _SaveLoadMapper = GetComponent<SaveLoadMapper>();
    }

    private void Start()
    {
        _Logger = new PrefixLogger(new UnityLogger(), "[SaveLoadBuffer]");
        _AffectedSaveTypes = new List<SaveLoadEnum.eSaveType>();
    }

    /// <summary>
    /// 読み取ったデータを格納
    /// </summary>
    /// <param name="loadData"></param>
    public void SetLoadData(byte[] loadData)
    {
        if (loadData == null || loadData.Length == 0)
        {
            _Logger.Log($"{loadData} is null or sizeoff");
            return;
        }
        if (_SaveLoadData == null || _SaveLoadData.Length != loadData.Length)
        {
            _SaveLoadData = new byte[loadData.Length];
        }

        Array.Copy(loadData, _SaveLoadData, loadData.Length);
    }

    /// <summary>
    /// すべてのデータを取得する
    /// </summary>
    public void Capture()
    {
        _AffectedSaveTypes.Clear();

        List<byte> saveData = new List<byte>();
        var saveTypeArray = Enum.GetValues(typeof(SaveLoadEnum.eSaveType));
        foreach(SaveLoadEnum.eSaveType type in saveTypeArray)
        {
            if (type == SaveLoadEnum.eSaveType.AllData) continue;
            _AffectedSaveTypes.Add(type);
            byte[] data = _SaveLoadMapper.FindRawSaveData(type);
            saveData.AddRange(data);
        }

        if (_SaveLoadData == null)
        {
            _SaveLoadData = new byte[saveData.Count];
        }
        _SaveLoadData = saveData.ToArray();

        _Logger.Log($"{_SaveLoadData} capture end");
    }


    /// <summary>
    /// 指定したモノだけ取得する
    /// </summary>
    public void Capture(List<SaveLoadEnum.eSaveType> saveList)
    {
        if (_SaveLoadData == null || _SaveLoadData.Length == 0)
        {
            Capture();
            return;
        }

        int startIndex, blockSize = 0;
        foreach (var saveType in saveList)
        {
            if (saveType == SaveLoadEnum.eSaveType.AllData)
            {
                _Logger.LogWarning($" {saveType} is inside saveList");
                Capture();
                _Logger.LogWarning($"{_SaveLoadData} was saved from Alldata located in {saveList}");
            }

            _AffectedSaveTypes.Add(saveType);
            GetSaveTypeInSaveLoadData(saveType, out startIndex, out blockSize);
            if(startIndex == -1 && blockSize == 0)
            {
                _Logger.LogWarning($"_SaveLoadData dont find {saveType}");
                Capture();
                _Logger.LogWarning($"{_SaveLoadData} was saved from Capture(saveList)");
                return;
            }
            else
            {
                byte[] newBlock = _SaveLoadMapper.FindRawSaveData(saveType);
                if (newBlock == null || newBlock.Length < 6)
                {
                    _Logger.LogWarning($"Invalid new block for {saveType}. Skip.");
                    continue;
                }
                if (newBlock.Length == blockSize)
                {
                    //上書きする
                    Array.Copy(newBlock, 0, _SaveLoadData, startIndex, blockSize);
                    _Logger.Log($"Updated {saveType} block in-place. len={blockSize}");
                }
                else
                {
                    //サイズ違いで差し替えが必要
                    List<byte> buffer = new List<byte>(_SaveLoadData.Length - blockSize + newBlock.Length);
                    //前半のデータを追加
                    buffer.AddRange(new ArraySegment<byte>(_SaveLoadData, 0, startIndex));
                    //新しいデータを追加
                    buffer.AddRange(newBlock);
                    //後半のデータを追加
                    int tailStart = startIndex + blockSize;
                    int tailLength = _SaveLoadData.Length - tailStart;
                    //不要なbyte[]の新規確保をせずに範囲指定コピーできる
                    buffer.AddRange(new ArraySegment<byte>(_SaveLoadData, tailStart, tailLength));

                    _SaveLoadData = buffer.ToArray();
                    _Logger.Log($"Replaced {saveType} block: old={blockSize}, new={newBlock.Length}, total={_SaveLoadData.Length}");

                }
            }
        }
        _Logger.Log($"capture end. total={_SaveLoadData?.Length ?? 0}, types=[{string.Join(",", saveList)}]");

    }


    /// <summary>
    /// セーブタイプのIndex位置をサイズの取得
    /// </summary>
    /// <param name="saveType"></param>
    /// <param name="startInex"></param>
    /// <param name="dataSize"></param>
    private void GetSaveTypeInSaveLoadData(SaveLoadEnum.eSaveType saveType,out int startIndex,out int blockSize)
    {
        startIndex = -1;
        blockSize = 0;

        int i = 0;
        //「何の」「いつの」「どのくらいの」「data」の順
        while (i < _SaveLoadData.Length)
        {
            // 範囲ガード（壊れ対策）
            if (i + 6 > _SaveLoadData.Length) return;

            byte type = _SaveLoadData[i];
            int sizeIndex = i + 2;//sizeの場所まで移動

            int index = sizeIndex;
            int payload = BitUtility.ReadInt(_SaveLoadData, ref index);

            int currentBlockSize = 1 + 1 + 4 + payload;// type + version + size(4byte) + payload
            if (i + currentBlockSize > _SaveLoadData.Length) return;

            if ((SaveLoadEnum.eSaveType)type == saveType)
            {
                startIndex = i;
                blockSize = currentBlockSize;   // ← これだけ返す
                return;
            }

            i += currentBlockSize; // 次ブロック
        }
    }
}

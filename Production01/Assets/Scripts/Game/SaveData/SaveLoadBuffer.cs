using UnityEngine;
using System;
using System.Collections.Generic;
using NUnit.Framework.Constraints;

public class SaveLoadBuffer : MonoBehaviour
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

        int startIndex, payloadSize = 0;
        foreach (var saveType in saveList)
        {
            if (saveType == SaveLoadEnum.eSaveType.AllData)
            {
                _Logger.LogWarning($" {saveType} is inside saveList");
                Capture();
                _Logger.LogWarning($"{_SaveLoadData} was saved from Alldata located in {saveList}");
            }

            _AffectedSaveTypes.Add(saveType);
            GetSaveTypeInSaveLoadData(saveType, out startIndex, out payloadSize);
            if(startIndex == -1 && payloadSize == 0)
            {
                _Logger.LogWarning($"_SaveLoadData dont find {saveType}");
                Capture();
                _Logger.LogWarning($"{_SaveLoadData} was saved from Capture(saveList)");
                break;
            }
            else
            {
                byte[] data = _SaveLoadMapper.FindRawSaveData(saveType);
                if(data.Length == payloadSize)
                {
                    //上書きする
                    Array.Copy(data, 0, _SaveLoadData, startIndex, payloadSize);
                    _Logger.Log($"Updated {saveType} block in-place.");
                }
                else
                {
                    //サイズ違いで差し替えが必要
                    List<byte> buffer = new List<byte>(_SaveLoadData.Length - payloadSize + data.Length);
                    //前半のデータを追加
                    buffer.AddRange(new ArraySegment<byte>(_SaveLoadData, 0, startIndex));
                    //新しいデータを追加
                    buffer.AddRange(data);
                    //後半のデータを追加
                    int tailStart = startIndex + payloadSize;
                    int tailLength = _SaveLoadData.Length - tailStart;
                    //不要なbyte[]の新規確保をせずに範囲指定コピーできる
                    buffer.AddRange(new ArraySegment<byte>(_SaveLoadData, tailStart, tailLength));

                    _SaveLoadData = buffer.ToArray();
                }
            }
        }
        _Logger.Log($"{_SaveLoadData}{saveList} capture end");

    }

    /// <summary>
    /// セーブタイプのIndex位置をサイズの取得
    /// </summary>
    /// <param name="saveType"></param>
    /// <param name="startInex"></param>
    /// <param name="payloadSize"></param>
    public void GetSaveTypeInSaveLoadData(SaveLoadEnum.eSaveType saveType,out int startIndex,out int payloadSize)
    {
        startIndex = -1;
        payloadSize = 0;

        int i = 0;
        //「何の」「いつの」「どのくらいの」「data」の順
        while (i < _SaveLoadData.Length)
        {
            byte type = _SaveLoadData[i];
            if ((SaveLoadEnum.eSaveType)type == saveType)
            {
                startIndex = i;

                int index = i + 2; // type(1) + version(1)
                payloadSize = BitUtility.ReadInt(_SaveLoadData, ref index);
                return;
            }
            else
            {
                int index = i + 2;
                int size = BitUtility.ReadInt(_SaveLoadData, ref index);

                // 次のtype位置へジャンプ
                i += 1 + 1 + 4 + size; // type + version + size(4byte) + payload
            }
        }
    }
}

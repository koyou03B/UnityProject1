using UnityEngine;
using System;
using System.Collections.Generic;
using NUnit.Framework.Constraints;

public class SaveLoadBuffer : MonoBehaviour
{
    private SaveLoadMapper _SaveLoadMapper;
    private PrefixLogger _Logger;
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
        List<byte> saveData = new List<byte>();
        var saveTypeArray = Enum.GetValues(typeof(SaveLoadEnum.eSaveType));
        foreach(SaveLoadEnum.eSaveType type in saveTypeArray)
        {
            if (type == SaveLoadEnum.eSaveType.AllData) continue;
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
            GetSaveTypeInSaveLoadData(saveType, out startIndex, out payloadSize);
            if(startIndex == -1 && payloadSize == 0)
            {
                _Logger.LogWarning($"_SaveLoadData dont find {saveType}");
                Capture();
                _Logger.LogWarning($"{_SaveLoadData} is all save");
                break;
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

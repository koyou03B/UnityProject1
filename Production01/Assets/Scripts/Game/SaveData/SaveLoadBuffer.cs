using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static SaveLoadEnum;

public partial class SaveLoadBuffer : MonoBehaviour
{
    private SaveLoadMapper _SaveLoadMapper;
    private PrefixLogger _Logger;
    //「変更があった部分」かつ「適用対象部分」として保存
    private List<SaveLoadEnum.eSaveType> _AffectedSaveTypes;//affected:影響を受けた
    private byte[] _SaveLoadData;

    /// <summary>
    /// 保存しているデータをそのまま参照で渡す（非同期処理用）
    /// </summary>
    public byte[] GetSaveLoadDataArray() => _SaveLoadData;

    public List<SaveLoadEnum.eSaveType> AffectedSaveTypes => _AffectedSaveTypes;

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
    /// 一度しか入らない想定
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
    /// 選択したスロットと上位層にいるSaveTypeを保存
    /// </summary>
    public void Capture(SaveLoadEnum.eSaveType slotType)
    {
        //更新候補を一度削除
        _AffectedSaveTypes.Clear();
        List<byte> saveData = new List<byte>();

        var sys = _SaveLoadMapper.FindRawSaveData(SaveLoadEnum.eSaveType.System);
        if (sys != null && sys.Length >= 6)
        {
            //データと更新候補の追加
            saveData.AddRange(sys);
            _AffectedSaveTypes.Add(eSaveType.System);
        }
        var input = _SaveLoadMapper.FindRawSaveData(SaveLoadEnum.eSaveType.Input);
        if (input != null && input.Length >= 6)
        {
            //データと更新候補の追加
            saveData.AddRange(input);
            _AffectedSaveTypes.Add(eSaveType.Input);
        }

        var saveTypeArray = Enum.GetValues(typeof(SaveLoadEnum.eSaveType));
        List<byte> innerData = new List<byte>();
        foreach (SaveLoadEnum.eSaveType type in saveTypeArray)
        {
            //インナーデータに必要ないものは見ない
            if(type == eSaveType.AllData || type == eSaveType.System || type == eSaveType.Input ||
                type == eSaveType.Slot1 || type == eSaveType.Slot2 || type == eSaveType.Slot3)
            {
                continue;
            }
            //インナーデータの取得
            var block = _SaveLoadMapper.FindRawSaveData(type);
            if(block != null && block.Length >= 6)
            {
                innerData.AddRange(block);
                _AffectedSaveTypes.Add(type);
            }
        }

        // 新しいSlotのブロックの作成
        byte[] slotBlock = BytePacker.Pack((byte)slotType, 0, innerData.ToArray());
        saveData.AddRange(slotBlock);

        _SaveLoadData = saveData.ToArray();
    }


    /// <summary>
    /// 部分キャプチャ
    /// 選んでいるSlotと指定されているSaveTypeだけを更新する
    /// </summary>
    public void Capture(SaveLoadEnum.eSaveType slotType, List<SaveLoadEnum.eSaveType> saveList)
    {
        if (saveList == null || saveList.Count == 0) return;

        if (_SaveLoadData == null || _SaveLoadData.Length == 0)
        {
            Capture(slotType); // 初回はフル
            _Logger.Log($"Capture(slot={slotType}, inner=...) done on empty data. total={_SaveLoadData?.Length ?? 0}");
            return;
        }
        //更新候補を一度削除
        _AffectedSaveTypes.Clear();

        foreach (var type in saveList)
        {
            //AllDataが含まれていたら全キャプチャに変更(入れてほしくはない)
            if (type == SaveLoadEnum.eSaveType.AllData)
            {
                Capture(slotType);
                return;
            }

            if(type == SaveLoadEnum.eSaveType.System || type == SaveLoadEnum.eSaveType.Input)
            {
                var newBlock = _SaveLoadMapper.FindRawSaveData(type);
                if (newBlock == null || newBlock.Length < 6)
                {
                    _Logger.LogWarning($"Skip {type}: invalid block.");
                    continue;
                }
                //トップ層の更新
                UpsertBlockAtEndOrReplace(type, newBlock);
                continue;
            }
            // Slotがあれば置換/追加,無ければ新規Slot作成
            UpsertInnerInSlot(slotType, type); 
        }

        _Logger.Log($"Capture(slot={slotType}, inner=[{string.Join(",", saveList)}]) end. total={_SaveLoadData.Length}");
    }

    /// <summary>
    /// Top層にあるセーブタイプの上書きもしくは追加
    /// </summary>
    /// <param name="saveType"></param>
    /// <param name="newBlock"></param>
    private void UpsertBlockAtEndOrReplace(SaveLoadEnum.eSaveType saveType, byte[] newBlock)
    {
        if (newBlock == null || newBlock.Length < 6) return;
        if ((SaveLoadEnum.eSaveType)newBlock[0] != saveType) return;

        // 既存を探す
        FindSaveTypeBlockInfo(saveType, out int startIndex, out int oldBlockSize);
        if (startIndex >= 0)
        {
            // 置換
            if (oldBlockSize == newBlock.Length)
            {
                Array.Copy(newBlock, 0, _SaveLoadData, startIndex, oldBlockSize); // in-place
                _AffectedSaveTypes.Add(saveType);
                return;
            }

            // 再構築
            var buf = new List<byte>(_SaveLoadData.Length - oldBlockSize + newBlock.Length);
            buf.AddRange(new ArraySegment<byte>(_SaveLoadData, 0, startIndex));
            buf.AddRange(newBlock);

            int tailStart = startIndex + oldBlockSize;
            int tailLen = _SaveLoadData.Length - tailStart;
            if (tailLen > 0)
            {
                buf.AddRange(new ArraySegment<byte>(_SaveLoadData, tailStart, tailLen));
            }

            _SaveLoadData = buf.ToArray();
            _AffectedSaveTypes.Add(saveType);
            return;
        }

        // なければ末尾に追記
        if (_SaveLoadData == null || _SaveLoadData.Length == 0)
        {
            _SaveLoadData = newBlock.ToArray();
        }
        else
        {
            var buf = new List<byte>(_SaveLoadData.Length + newBlock.Length);
            buf.AddRange(_SaveLoadData);
            buf.AddRange(newBlock);
            _SaveLoadData = buf.ToArray();
        }
        _AffectedSaveTypes.Add(saveType);
    }

    /// <summary>
    /// SlotNのpayload内でinnerTypeブロックを上書きもしくは追加する。
    /// innerNewBlockは[type][ver][size][payload] の完全ブロックを想定。
    /// SlotN が無い場合は末尾にSlotN新規追加
    /// </summary>
    private void UpsertInnerInSlot(SaveLoadEnum.eSaveType slotType,SaveLoadEnum.eSaveType innerType)
    {
        // 新しいinnerブロックをMapperから取得
        byte[] innerNewBlock = _SaveLoadMapper.FindRawSaveData(innerType);
        if (innerNewBlock == null || innerNewBlock.Length < 6)
        {
            _Logger.LogWarning("invalid innerNewBlock"); 
            return;
        }

        //slotTypeを探す
        FindSaveTypeBlockInfo(slotType, out int slotStart, out int slotBlockSize);
        //スロットが存在しない
        if (slotStart < 0)
        {
            byte[] slotBlock = BytePacker.Pack((byte)slotType, 0, innerNewBlock);
            if (_SaveLoadData == null || _SaveLoadData.Length == 0)
            {
                _SaveLoadData = slotBlock;
            }
            else
            {
                //あるなら後ろに追加
                var buf = new List<byte>(_SaveLoadData.Length + slotBlock.Length);
                buf.AddRange(_SaveLoadData);
                buf.AddRange(slotBlock);
                _SaveLoadData = buf.ToArray();
            }
            _AffectedSaveTypes.Add(innerType);

            _Logger.Log($"Append new {slotType} (payload={slotBlock.Length})");
            return;
        }

        //slotのペイロード範囲の特定
        byte slotVersion = _SaveLoadData[slotStart + 1];
        int slotPayloadStart = slotStart + 6;
        int slotPayloadLen = slotBlockSize - 6;

        //ペイロード内でinnerTypeが存在するか探す
        if(TryFindInnerInPayload(_SaveLoadData, slotPayloadStart, slotPayloadLen,
                              innerType, out int innerAbsStart, out int innerOldSize))
        {
            if (innerOldSize == innerNewBlock.Length)
            {
                // 同じサイズなのでその場置換
                Array.Copy(innerNewBlock, 0, _SaveLoadData, innerAbsStart, innerOldSize);
                _AffectedSaveTypes.Add(innerType);

                _Logger.Log($"Updated {slotType}/{innerType} in-place (len={innerOldSize})");
                return;
            }
            else
            {
                //サイズが違う
                int relStart = innerAbsStart - slotPayloadStart;
                var newPayload = new List<byte>(slotPayloadLen - innerOldSize + innerNewBlock.Length);
                newPayload.AddRange(new ArraySegment<byte>(_SaveLoadData, slotPayloadStart, relStart));
                newPayload.AddRange(innerNewBlock);
                int tailStart = relStart + innerOldSize;
                int tailLen = slotPayloadLen - tailStart;
                if (tailLen > 0)
                {
                    newPayload.AddRange(new ArraySegment<byte>(_SaveLoadData, slotPayloadStart + tailStart, tailLen));
                }
                
                // 新しいSlotのブロックの作成
                byte[] slotBlock = BytePacker.Pack((byte)slotType, slotVersion, newPayload.ToArray());
                ReplaceBlockAt(slotStart, slotBlockSize, slotBlock);
                _AffectedSaveTypes.Add(innerType);

                _Logger.Log($"Rebuilt {slotType} (oldPayload={slotPayloadLen} -> newPayload={newPayload.Count})");
                return;
            }
        }
        else
        {
            //innerTypeがない場合
            var newPayload = new List<byte>(slotPayloadLen + innerNewBlock.Length);
            newPayload.AddRange(new ArraySegment<byte>(_SaveLoadData, slotPayloadStart, slotPayloadLen));
            newPayload.AddRange(innerNewBlock);

            byte[] slotBlock = BytePacker.Pack((byte)slotType, slotVersion, newPayload.ToArray());

            ReplaceBlockAt(slotStart, slotBlockSize, slotBlock);
            _AffectedSaveTypes.Add(innerType);

            _Logger.Log($"Appended inner {innerType} to {slotType} (payload {slotPayloadLen} -> {newPayload.Count})");
            return;
        }
    }

    /// <summary>
    /// Slotブロック全体を置換(同サイズはin-place、違えば前+新+後で再構築)
    /// </summary>
    /// <param name="startIndex"></param>
    /// <param name="oldBlockSize"></param>
    /// <param name="newBlock"></param>
    private void ReplaceBlockAt(int startIndex, int oldBlockSize, byte[] newBlock)
    {
        if (oldBlockSize == newBlock.Length)
        {
            Array.Copy(newBlock, 0, _SaveLoadData, startIndex, oldBlockSize);
            return;
        }
        //再構築
        var buf = new List<byte>(_SaveLoadData.Length - oldBlockSize + newBlock.Length);
        buf.AddRange(new ArraySegment<byte>(_SaveLoadData, 0, startIndex));
        buf.AddRange(newBlock);
        int tailStart = startIndex + oldBlockSize;
        int tailLen = _SaveLoadData.Length - tailStart;
        if (tailLen > 0)
        {
            buf.AddRange(new ArraySegment<byte>(_SaveLoadData, tailStart, tailLen));
        }
        _SaveLoadData = buf.ToArray();
        _Logger.Log($"Replaced Slot block: old={oldBlockSize}, new={newBlock.Length}, total={_SaveLoadData.Length}");

    }

    /// <summary>
    /// SlotやSystemなどトップレベルのブロック位置を取る
    /// </summary>
    /// <param name="saveType"></param>
    /// <param name="startIndex"></param>
    /// <param name="blockSize"></param>
    private void FindSaveTypeBlockInfo(SaveLoadEnum.eSaveType saveType, out int startIndex, out int blockSize)
    {
        startIndex = -1;
        blockSize = 0;
        if (_SaveLoadData == null) return;

        int i = 0;
        while (i < _SaveLoadData.Length)
        {
            //ブロックサイズの取得
            if (!TryReadBlockSize(_SaveLoadData, i, out int bs)) return;
            if ((SaveLoadEnum.eSaveType)_SaveLoadData[i] == saveType)
            {
                startIndex = i;
                blockSize = bs;
                return;
            }
            i += bs;
        }
    }

    /// <summary>
    /// SlotPayload内でSaveTypeを探す
    /// </summary>
    /// <param name="data"></param>
    /// <param name="payloadStart"></param>
    /// <param name="payloadLen"></param>
    /// <param name="innerType"></param>
    /// <param name="innerAbsStart"></param>
    /// <param name="innerBlockSize"></param>
    /// <returns></returns>
    private bool TryFindInnerInPayload(byte[] data, int payloadStart, int payloadLen,
                                              SaveLoadEnum.eSaveType innerType,
                                              out int innerAbsStart, out int innerBlockSize)
    {
        innerAbsStart = -1; innerBlockSize = 0;
        int end = payloadStart + payloadLen;
        int i = payloadStart;
        while (i < end)
        {
            if (!TryReadBlockSize(data, i, out int bs)) return false;
            if ((SaveLoadEnum.eSaveType)data[i] == innerType)
            {
                innerAbsStart = i; 
                innerBlockSize = bs; 
                return true;
            }
            i += bs;
        }
        return false; // 見つからず
    }

    /// <summary>
    /// [type:1][version:1][size:4][payload:size]のtotalサイズを読む
    /// </summary>
    /// <param name="data"></param>
    /// <param name="offset"></param>
    /// <param name="blockSize"></param>
    /// <returns></returns>
    private bool TryReadBlockSize(byte[] data, int offset, out int blockSize)
    {
        blockSize = 0;
        if (data == null || offset < 0 || offset + 6 > data.Length) return false;

        int idx = offset + 2; // size位置
        int payload = BitUtility.ReadInt(data, ref idx);
        if (payload < 0) return false;

        blockSize = 1 + 1 + 4 + payload;
        return offset + blockSize <= data.Length;
    }
}

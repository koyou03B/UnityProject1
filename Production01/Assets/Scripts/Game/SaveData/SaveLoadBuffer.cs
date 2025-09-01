using System;
using System.Collections.Generic;
using UnityEngine;
using static SaveLoadEnum;

public partial class SaveLoadBuffer : MonoBehaviour
{
    private SaveLoadMapper _SaveLoadMapper;
    private PrefixLogger _Logger;
    //「変更があった部分」かつ「適用対象部分」として保存
    private List<SaveLoadTags.eInnerTypeTag> _AffectedSaveTypes;//affected:影響を受けた
    private byte[] _SaveLoadData;

    /// <summary>
    /// 保存しているデータをそのまま参照で渡す（非同期処理用）
    /// </summary>
    public byte[] GetSaveLoadDataArray() => _SaveLoadData;

    public List<SaveLoadTags.eInnerTypeTag> AffectedSaveTypes => _AffectedSaveTypes;

    private void Awake()
    {
        _SaveLoadMapper = GetComponent<SaveLoadMapper>();
    }

    public void Setup()
    {
        _Logger = new PrefixLogger(new UnityLogger(), "[SaveLoadBuffer]");
        _AffectedSaveTypes = new List<SaveLoadTags.eInnerTypeTag>();
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
    public void Capture(SaveLoadTags.eTopTag slotType)
    {
        //更新候補を一度削除
        _AffectedSaveTypes.Clear();
        List<byte> saveData = new List<byte>();
        //Systemコンテナの構築
        var systemInner = new List<byte>();
        var saveTypeArray = Enum.GetValues(typeof(SaveLoadTags.eInnerTypeTag));
        foreach (SaveLoadTags.eInnerTypeTag inner in saveTypeArray)
        {
            var block = _SaveLoadMapper.FindRawSaveData(inner);
            if (block != null && block.Length >= 6)
            {
                systemInner.AddRange(block);
                _AffectedSaveTypes.Add(inner);
            }

            //Input以外のシステム内容(絶対増える)
            if (inner == SaveLoadTags.eInnerTypeTag.GameSystem) break;
        }
        byte[] systemBlock = BytePacker.Pack((byte)SaveLoadTags.eTopTag.General, 0, systemInner.ToArray());
        saveData.AddRange(systemBlock);

        //SlotNコンテナ
        var slotInner = new List<byte>();
        foreach (SaveLoadTags.eInnerTypeTag inner in saveTypeArray)
        {
            //Input以外のシステム内容(絶対増える)
            if (inner == SaveLoadTags.eInnerTypeTag.Input ||
                inner == SaveLoadTags.eInnerTypeTag.Option ||
                inner == SaveLoadTags.eInnerTypeTag.GameSystem) continue;

            var block = _SaveLoadMapper.FindRawSaveData(inner);
            if (block != null && block.Length >= 6)
            {
                slotInner.AddRange(block);
                _AffectedSaveTypes.Add(inner);
            }
        }
        byte[] slotBlock = BytePacker.Pack((byte)slotType, 0, slotInner.ToArray());
        saveData.AddRange(slotBlock);

        _SaveLoadData = saveData.ToArray();
    }


    /// <summary>
    /// 部分キャプチャ
    /// 選んでいるSlotと指定されているSaveTypeだけを更新する
    /// </summary>
    public void Capture(SaveLoadTags.eTopTag slotType, List<SaveLoadTags.eInnerTypeTag> innerList)
    {
        if (innerList == null || innerList.Count == 0) return;

        if (_SaveLoadData == null || _SaveLoadData.Length == 0)
        {
            Capture(slotType); // 初回はフル
            _Logger.Log($"Capture(slot={slotType}, inner=...) done on empty data. total={_SaveLoadData?.Length ?? 0}");
            return;
        }
        //更新候補を一度削除
        _AffectedSaveTypes.Clear();

        //innerListを保存
        foreach (var inner in innerList)
        {
            if (inner == SaveLoadTags.eInnerTypeTag.Input ||
                inner == SaveLoadTags.eInnerTypeTag.Option ||
                inner == SaveLoadTags.eInnerTypeTag.GameSystem)
            {
                UpsertInnerInSlot(SaveLoadTags.eTopTag.General, inner);
                continue;
            }
            // Slotがあれば置換/追加,無ければ新規Slot作成
            UpsertInnerInSlot(slotType, inner); 
        }

        _Logger.Log($"Capture(slot={slotType}, inner=[{string.Join(",", innerList)}]) end. total={_SaveLoadData.Length}");
    }

    /// <summary>
    /// SlotNのpayload内でinnerTypeブロックを上書きもしくは追加する。
    /// innerNewBlockは[type][ver][size][payload] の完全ブロックを想定。
    /// SlotN が無い場合は末尾にSlotN新規追加
    /// </summary>
    private void UpsertInnerInSlot(SaveLoadTags.eTopTag containerTop,SaveLoadTags.eInnerTypeTag innerType)
    {
        // 新しいinnerブロックをMapperから取得
        byte[] innerNewBlock = _SaveLoadMapper.FindRawSaveData(innerType);
        if (innerNewBlock == null || innerNewBlock.Length < 6)
        {
            _Logger.LogWarning("invalid innerNewBlock"); 
            return;
        }

        //Topのコンテナを探す
        TryFindTopBlockInfo(containerTop, out int topStart, out int topBlockSize);
        //Topのコンテナが存在しない
        if (topStart < 0)
        {
            byte[] topBlock = BytePacker.Pack((byte)containerTop, 0, innerNewBlock);
            if (_SaveLoadData == null || _SaveLoadData.Length == 0)
            {
                _SaveLoadData = topBlock;
            }
            else
            {
                //あるなら後ろに追加
                var buf = new List<byte>(_SaveLoadData.Length + topBlock.Length);
                buf.AddRange(_SaveLoadData);
                buf.AddRange(topBlock);
                _SaveLoadData = buf.ToArray();
            }
            _AffectedSaveTypes.Add(innerType);

            _Logger.Log($"Append new {containerTop} (payload={topBlock.Length})");
            return;
        }

        //slotのペイロード範囲の特定
        byte topVersion = _SaveLoadData[topStart + 1];
        int payloadStart = topStart + 6;
        int payloadLen = topBlockSize - 6;

        //ペイロード内にinnerTypeが存在するか探す
        if(TryFindInnerInPayload(_SaveLoadData, payloadStart, payloadLen,
                              innerType, out int innerAbsStart, out int innerOldSize))
        {
            if (innerOldSize == innerNewBlock.Length)
            {
                // 同じサイズなのでその場置換
                Array.Copy(innerNewBlock, 0, _SaveLoadData, innerAbsStart, innerOldSize);
                _AffectedSaveTypes.Add(innerType);

                _Logger.Log($"Updated {containerTop}/{innerType} in-place (len={innerOldSize})");
                return;
            }
            else
            {
                //サイズが違う
                int relStart = innerAbsStart - payloadStart;
                var newPayload = new List<byte>(payloadLen - innerOldSize + innerNewBlock.Length);
                newPayload.AddRange(new ArraySegment<byte>(_SaveLoadData, payloadStart, relStart));
                newPayload.AddRange(innerNewBlock);
                int tailStart = relStart + innerOldSize;
                int tailLen = payloadLen - tailStart;
                if (tailLen > 0)
                {
                    newPayload.AddRange(new ArraySegment<byte>(_SaveLoadData, payloadStart + tailStart, tailLen));
                }
                
                // 新しいSlotのブロックの作成
                byte[] topBlock = BytePacker.Pack((byte)containerTop, topVersion, newPayload.ToArray());
                ReplaceBlockAt(topStart, topBlockSize, topBlock);
                _AffectedSaveTypes.Add(innerType);

                _Logger.Log($"Rebuilt {containerTop} (oldPayload={payloadLen} -> newPayload={newPayload.Count})");
                return;
            }
        }
        else
        {
            //innerTypeがない場合
            var newPayload = new List<byte>(payloadLen + innerNewBlock.Length);
            newPayload.AddRange(new ArraySegment<byte>(_SaveLoadData, payloadStart, payloadLen));
            newPayload.AddRange(innerNewBlock);

            byte[] topBlock = BytePacker.Pack((byte)containerTop, topVersion, newPayload.ToArray());

            ReplaceBlockAt(topStart, topBlockSize, topBlock);
            _AffectedSaveTypes.Add(innerType);

            _Logger.Log($"Appended inner {innerType} to {containerTop} (payload {payloadLen} -> {newPayload.Count})");
            return;
        }
    }

    /// <summary>
    /// Topブロック全体を置換(同サイズはin-place、違えば前+新+後で再構築)
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
    /// <param name="topTag"></param>
    /// <param name="startIndex"></param>
    /// <param name="blockSize"></param>
    private void TryFindTopBlockInfo(SaveLoadTags.eTopTag topTag, out int startIndex, out int blockSize)
    {
        startIndex = -1;
        blockSize = 0;
        if (_SaveLoadData == null) return;

        int i = 0;
        while (i < _SaveLoadData.Length)
        {
            //ブロックサイズの取得
            if (!TryReadBlockSize(_SaveLoadData, i, out int bs)) return;
            if ((SaveLoadTags.eTopTag)_SaveLoadData[i] == topTag)
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
                                              SaveLoadTags.eInnerTypeTag innerType,
                                              out int innerAbsStart, out int innerBlockSize)
    {
        innerAbsStart = -1; innerBlockSize = 0;
        int end = payloadStart + payloadLen;
        int i = payloadStart;
        while (i < end)
        {
            if (!TryReadBlockSize(data, i, out int bs)) return false;
            if ((SaveLoadTags.eInnerTypeTag)data[i] == innerType)
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

    // SaveLoadBuffer.cs のクラス内に追加
    private static readonly SaveLoadTags.eTopTag[] TopBlockOrder = new[] {
    SaveLoadTags.eTopTag.General,
    SaveLoadTags.eTopTag.Slot1,
    SaveLoadTags.eTopTag.Slot2,
    SaveLoadTags.eTopTag.Slot3,
};
    private void SortTopBlocks()
    {
        if (_SaveLoadData == null || _SaveLoadData.Length < 6) return;
        Dictionary<SaveLoadTags.eTopTag, byte[]> firstSeen = new Dictionary<SaveLoadTags.eTopTag, byte[]>();

        int i = 0;
        while (i < _SaveLoadData.Length)
        {
            if (!TryReadBlockSize(_SaveLoadData, i, out int bs))
            {
                _Logger?.LogWarning($"SortTopBlocks: bad block at {i}");
                return;
            }
            var tagByte = _SaveLoadData[i];
            var isTop = System.Enum.IsDefined(typeof(SaveLoadTags.eTopTag), (SaveLoadTags.eTopTag)tagByte);
            if (isTop)
            {
                var top = (SaveLoadTags.eTopTag)tagByte;
                if (!firstSeen.ContainsKey(top))
                {
                    firstSeen[top] = new System.ArraySegment<byte>(_SaveLoadData, i, bs).ToArray();
                }
            }

            i += bs;
        }

        //順序の再構成
        List<byte> buf = new List<byte>(_SaveLoadData.Length);
        foreach (var t in TopBlockOrder)
        {
            if (firstSeen.TryGetValue(t, out var block))
            {
                buf.AddRange(block);
            }
        }

        // 変化がなければスキップ
        if (buf.Count != _SaveLoadData.Length || !ByteArrayEquals(_SaveLoadData, 0, buf))
        {
            _SaveLoadData = buf.ToArray();
            _Logger?.Log("SortTopBlocks: reordered");
        }


        bool ByteArrayEquals(byte[] src, int offset, List<byte> other)
        {
            if (src == null) return other == null || other.Count == 0;
            if (offset + other.Count > src.Length) return false;

            for (int k = 0; k < other.Count; k++)
            {
                if (src[offset + k] != other[k]) return false;
            }

            return true;
        }
    }
}

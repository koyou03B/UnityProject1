using System;
using System.Collections.Generic;
using UnityEngine;

public partial class SaveLoadBuffer : MonoBehaviour
{
    /// <summary>
    /// dataからsaveTypeのサイズ分に変更
    /// </summary>
    /// <param name="data"></param>
    /// <param name="saveType"></param>
    /// <returns></returns>
    public bool TrimToSingleBlock(ref byte[] data, SaveLoadTags.eTopTag topTag)
    {
        if (data == null || data.Length < 6)
            return false;

        int i = 0;
        while (i < data.Length)
        {
            // 壊れ対策: ヘッダが置けないサイズなら終了
            if (i + 6 > data.Length) return false;

            byte type = data[i];
            // size 読み取り
            int idx = i + 2;
            int payload = BitUtility.ReadInt(data, ref idx);
            int blockSize = 1 + 1 + 4 + payload; // header(6) + payload

            // 壊れ対策: ブロックが配列をはみ出す
            if (blockSize <= 0 || i + blockSize > data.Length) return false;

            if ((SaveLoadTags.eTopTag)type == topTag)
            {
                // 目的のブロックだけにリサイズ
                if (i == 0 && blockSize == data.Length)
                {
                    // 既に単一ブロック
                    return true;
                }

                data = new ArraySegment<byte>(data, i, blockSize).ToArray();
                return true;
            }

            i += blockSize;
        }

        // 見つからず
        return false;
    }

    /// <summary>
    /// スロットが存在するかどうか
    /// </summary>
    /// <param name="saveSlot"></param>
    /// <returns></returns>
    public bool ContainsTopBlock(SaveLoadTags.eTopTag topTag)
    {
        TryFindTopBlockInfo(topTag, out int startIndex, out int oldBlockSize);
        return startIndex != -1 && oldBlockSize != 0;
    }

    /// <summary>
    /// SaveDataTypeで区切ったブロックのサイズ分だけで構成しなおす
    /// トップ層限定
    /// </summary>
    /// <param name="topType"></param>
    /// <param name="data"></param>
    public void GetTopBlock(SaveLoadTags.eTopTag topType, ref byte[] data)
    {
        int startIndex = 0;
        int blockSize = 0;

        GetSaveTypeInSaveLoadData(topType, out startIndex, out blockSize);
        if (startIndex == -1 && blockSize == 0)
        {
            data = BytePacker.Pack((byte)topType, 0, Array.Empty<byte>());
            return;
        }

        data = new ArraySegment<byte>(_SaveLoadData, startIndex, blockSize).ToArray();
    }

    /// <summary>
    /// セーブタイプのIndex位置をサイズの取得
    /// Top層限定
    /// </summary>
    /// <param name="saveType"></param>
    /// <param name="startInex"></param>
    /// <param name="dataSize"></param>
    private void GetSaveTypeInSaveLoadData(SaveLoadTags.eTopTag saveType, out int startIndex, out int blockSize)
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

            if ((SaveLoadTags.eTopTag)type == saveType)
            {
                startIndex = i;
                blockSize = currentBlockSize;   //これだけ返す
                return;
            }

            i += currentBlockSize; //次ブロック
        }
    }


    /// <summary>
    /// スロット内のデータでほしいものだけを区切ったサイズ分で構成しなおす
    /// </summary>
    /// <param name="slotType"></param>
    /// <param name="innerType"></param>
    /// <param name="data"></param>
    public void GetInnerBlock(SaveLoadTags.eTopTag container, SaveLoadTags.eInnerTypeTag inner, ref byte[] data)
    {
        //スロットの位置とサイズをもらう
        TryFindTopBlockInfo(container, out int slotStart, out int slotBlockSize);

        //slotのペイロード範囲の特定
        int slotPayloadStart = slotStart + 6;
        int slotPayloadLen = slotBlockSize - 6;

        //ペイロード内でinnerTypeのサイズと位置をもらう
        if(TryFindInnerInPayload(_SaveLoadData, slotPayloadStart, slotPayloadLen,
                              inner, out int innerAbsStart, out int innerOldSize))
        {
            data = new ArraySegment<byte>(_SaveLoadData, innerAbsStart, innerOldSize).ToArray();
        }
        else
        {
            data = BytePacker.Pack((byte)inner, 0, Array.Empty<byte>());
        }
    }
}

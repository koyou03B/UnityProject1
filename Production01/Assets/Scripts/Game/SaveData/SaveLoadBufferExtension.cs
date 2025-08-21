using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public partial class SaveLoadBuffer : MonoBehaviour
{
    /// <summary>
    /// dataからsaveTypeのサイズ分に変更
    /// </summary>
    /// <param name="data"></param>
    /// <param name="saveType"></param>
    /// <returns></returns>
    public bool TrimToSingleBlock(ref byte[] data, SaveLoadEnum.eSaveType saveType)
    {
        if (data == null || data.Length < 6)
            return false;

        int i = 0;
        while (i < data.Length)
        {
            // 壊れ対策: ヘッダが置けないサイズなら終了
            if (i + 6 > data.Length) return false;

            byte type = data[i];
            int sizeIndex = i + 2;

            // size 読み取り
            int idx = sizeIndex;
            int payload = BitUtility.ReadInt(data, ref idx);
            int blockSize = 1 + 1 + 4 + payload; // header(6) + payload

            // 壊れ対策: ブロックが配列をはみ出す
            if (blockSize <= 0 || i + blockSize > data.Length) return false;

            if ((SaveLoadEnum.eSaveType)type == saveType)
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
    /// SaveDataTypeで区切った部分だけ取り出す
    /// </summary>
    /// <param name="saveType"></param>
    /// <param name="data"></param>
    public void FindSaveLoadDataArray(SaveLoadEnum.eSaveType saveType, ref byte[] data)
    {
        int startIndex = 0;
        int blockSize = 0;

        GetSaveTypeInSaveLoadData(saveType, out startIndex, out blockSize);
        if (startIndex == -1 && blockSize == 0)
        {
            data = new byte[0];
            return;
        }

        data = new byte[blockSize];
        data = new ArraySegment<byte>(_SaveLoadData, startIndex, startIndex + blockSize).ToArray();
    }

}

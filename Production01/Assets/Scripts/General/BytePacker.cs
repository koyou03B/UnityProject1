using System;
using System.Collections.Generic;
using UnityEngine;

public static class BytePacker
{
    private static ILogger _Logger = new PrefixLogger(new UnityLogger(), "[BytePacker]");
    public static byte[] Pack(byte type, byte version, byte[] payload)
    {
        int offset = 0;
        int size = 0;
        if (payload == null || payload.Length == 0)
        {
            size = 0;
        }
        else
        {
            size = payload.Length;
        }
        //「何の」「いつの」「どのくらいの」「data」の順
        byte[] result = new byte[1 + 1 + 4 + size];
        result[offset++] = type;
        result[offset++] = version;
        BitUtility.WriteInt(result, size, ref offset);
        if (payload == null || payload.Length == 0)
        {
            return result;
        }
        Buffer.BlockCopy(payload, 0, result, offset, size);

        return result;
    }

    public static bool TryUnpack(byte[] packedData, out byte type, out byte version, out byte[] payload)
    {
        type = 0;
        version = 0;

        if (packedData == null || packedData.Length < 6)
        {
            payload = new byte[0];
            _Logger.Log("Packed data is too short.");
            return false;
        }

        int index = 0;

        type = packedData[index++];
        version = packedData[index++];
        int size = BitUtility.ReadInt(packedData,ref index);

        //size +6(「何の」「いつの」「どのくらいの」) でpackdeDataと同じ長さ
        if (packedData.Length != size + 6)
        {
            payload = new byte[0];
            _Logger.LogError("Data size mismatch.");
            return false;
        }

        payload = new byte[size];
        Buffer.BlockCopy(packedData, 6, payload, 0, size);
        return true;
    }

}

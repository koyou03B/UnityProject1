using System;
using System.Collections.Generic;
using UnityEngine;

public static class BytePacker
{
    private static ILogger _Logger = new PrefixLogger(new UnityLogger(), "[BytePacker]");

    public static byte[] Pack(byte type, byte version, List<int> data)
    {
        var payload = new List<byte>();
        foreach (int value in data)
        {
            payload.AddRange(BitConverter.GetBytes(value));
        }

        int size = payload.Count;
        byte[] sizeBytes = BitConverter.GetBytes(size);

        //「何の」「いつの」「どのくらいの」「data」の順
        byte[] result = new byte[1 + 1 + 4 + size];
        result[0] = type;
        result[1] = version;
        Buffer.BlockCopy(sizeBytes, 0, result, 2, 4);
        Buffer.BlockCopy(payload.ToArray(), 0, result, 6, size);

        return result;
    }

    public static bool TryUnpack(byte[] packedData, out byte type, out byte version, out List<int> data)
    {
        data = new List<int>();
        type = 0;
        version = 0;

        if (packedData == null || packedData.Length < 6)
        {
            _Logger.LogError("Packed data is too short.");
            return false;
        }

        type = packedData[0];
        version = packedData[1];
        int size = BitConverter.ToInt32(packedData, 2);

        if (packedData.Length != size + 6)
        {
            _Logger.LogError("Data size mismatch.");
            return false;
        }

        for (int i = 0; i < size; i += 4)
        {
            if (6 + i + 4 > packedData.Length)
            {
                Debug.LogWarning("Truncated data at end of packet.");
                break;
            }

            int val = BitConverter.ToInt32(packedData, 6 + i);
            data.Add(val);
        }

        return true;
    }
}

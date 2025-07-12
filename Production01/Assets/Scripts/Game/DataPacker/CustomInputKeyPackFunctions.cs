using System.Collections.Generic;
using UnityEngine;

public partial class CustomInputKeyPacker
{
    private byte[] PackVersion0(List<int> keys)
    {
        var payload = new List<byte>();
        foreach (int value in keys)
        {
            BitUtility.WriteInt(payload, value);
        }
        return payload.ToArray();
    }

    private bool UnPackVersion0(byte[] payload, List<int> slots)
    {
        if (payload == null) return false;
        int offset = 0;
        for (int i = 0; i < payload.Length; i++)
        {
            slots.Add(BitUtility.ReadInt(payload, ref offset));
        }

        return true;
    }
}

using UnityEngine;
using System.Collections.Generic;
using System;
public partial class CustomInputKeyPacker : IDataPacker<List<int>>
{
    public const byte Version0 = 0;
    public const int SlotCountV0 = 5;

    private ILogger _Logger = new PrefixLogger(new UnityLogger(), "[CustomInputKeyPacker]");
    public byte[] PackPayload(List<int> slots, byte version)
    {
        switch (version)
        {
            case Version0:
                return PackVersion0(slots);
        }
        _Logger.LogError($"Unsupported version: {version}");
        return new byte[0];
    }

    public bool TryUnpackPayload(ReadOnlySpan<byte> payload, byte version, out List<int> slots)
    {
        slots = new List<int>();
        switch (version)
        {
            case Version0:
                return UnPackVersion0(payload.ToArray(), slots);
        }
        _Logger.LogError($"Unsupported version: {version}");
        return false;
    }
}

using System;
using UnityEngine;

public interface IDataPacker<T>
{
    byte[] PackPayload(T data, byte version);
    bool TryUnpackPayload(ReadOnlySpan<byte> payload, byte version, out T data);

}

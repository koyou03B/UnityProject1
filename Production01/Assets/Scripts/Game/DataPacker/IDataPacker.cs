using UnityEngine;

public interface IDataPacker<T>
{
    byte[] PackPayload(T data, byte version);
    bool TryUnpackPayload(byte[] bytes, byte version, out T data);

}

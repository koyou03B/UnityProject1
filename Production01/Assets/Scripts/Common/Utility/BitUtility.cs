using System;
using System.Collections.Generic;
using UnityEngine;

public static class BitUtility
{
    public static void WriteInt(List<byte> target, int value)
    {
        target.Add((byte)(value & 0xFF));
        target.Add((byte)((value >> 8) & 0xFF));
        target.Add((byte)((value >> 16) & 0xFF));
        target.Add((byte)((value >> 24) & 0xFF));
    }
    public static void WriteInt(byte[] target, int value,ref int offset)
    {
        target[offset++] = (byte)(value & 0xFF);
        target[offset++] = (byte)((value >> 8) & 0xFF);
        target[offset++] = (byte)((value >> 16) & 0xFF);
        target[offset++] = (byte)((value >> 24) & 0xFF);
    }

    public static int ReadInt(byte[] source, ref int offset)
    {
        int value = (source[offset]) |
                    (source[offset + 1] << 8) |
                    (source[offset + 2] << 16) |
                    (source[offset + 3] << 24);

        offset += 4;
        return value;
    }

    public static void WriteUInt(List<byte> target, uint value)
    {
        target.Add((byte)(value & 0xFF));
        target.Add((byte)((value >> 8) & 0xFF));
        target.Add((byte)((value >> 16) & 0xFF));
        target.Add((byte)((value >> 24) & 0xFF));
    }
    public static void WriteUInt(List<byte> target, uint value,ref int offset)
    {
        target[offset++] = (byte)(value & 0xFF);
        target[offset++] = (byte)((value >> 8) & 0xFF);
        target[offset++] = (byte)((value >> 16) & 0xFF);
        target[offset++] = (byte)((value >> 24) & 0xFF);
    }

    public static uint ReadUInt(byte[] source, ref int offset)
    {
        uint value = (uint)source[offset] |
                     ((uint)source[offset + 1] << 8) |
                     ((uint)source[offset + 2] << 16) |
                     ((uint)source[offset + 3] << 24);

        offset += 4;
        return value;
    }

    public static void WriteFloat(List<byte> target, float value)
    {
        int bits = BitConverter.SingleToInt32Bits(value);
        WriteInt(target, bits);
    }

    public static float ReadFloat(byte[] source, ref int offset)
    {
        int bits = ReadInt(source, ref offset);
        return BitConverter.Int32BitsToSingle(bits);
    }
}

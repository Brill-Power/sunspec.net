/*
 * Copyright (c) 2024 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

namespace SunSpec.Client;

public static class ModbusBinaryConversion
{
    public static ushort ReadUShort(ReadOnlySpan<byte> bytes)
        => BinaryPrimitives.ReadUInt16BigEndian(bytes);

    public static short ReadShort(ReadOnlySpan<byte> bytes)
        => BinaryPrimitives.ReadInt16BigEndian(bytes);

    public static uint ReadUInt(ReadOnlySpan<byte> bytes)
        => BinaryPrimitives.ReadUInt32BigEndian(bytes);

    public static string ReadString(ReadOnlySpan<byte> bytes)
        => new string(MemoryMarshal.Cast<byte, char>(bytes.Slice(0, bytes.Length - 1)));
}
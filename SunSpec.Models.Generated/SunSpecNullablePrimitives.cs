/*
 * Copyright (c) 2025 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using System.Buffers.Binary;
using System.Text;

namespace SunSpec.Models.Generated;

public static class SunSpecNullablePrimitives
{
    private static class Null
    {
        public const short Int16 = -0x8000;
        public const ushort UInt16 = 0xffff;
        public const ushort Accumulator16 = 0x0000;
        public const ushort Scale = 0x8000;
        public const int Int32 = -0x80000000;
        public const int Single = 0x7fc00000; // aka NaN
        public const uint UInt32 = 0xffffffff;
        public const uint Accumulator32 = 0x00000000;
        public const long Int64 = -0x8000000000000000;
        public const ulong UInt64 = 0xffffffffffffffff;
        public const ulong Accumulator64 = 0x0000000000000000;
        public const string String = "\x00";
    }

    public static float? ReadSingleBigEndian(ReadOnlySpan<byte> source)
    {
        float value = BinaryPrimitives.ReadSingleBigEndian(source);
        return Single.IsNaN(value) ? null : value;
    }

    public static void WriteSingleBigEndian(Span<byte> destination, float? value)
    {
        BinaryPrimitives.WriteSingleBigEndian(destination, value ?? Single.NaN);
    }

    public static double? ReadDoubleBigEndian(ReadOnlySpan<byte> source)
    {
        double value = BinaryPrimitives.ReadDoubleBigEndian(source);
        return Double.IsNaN(value) ? null : value;
    }

    public static void WriteDoubleBigEndian(Span<byte> destination, double? value)
    {
        BinaryPrimitives.WriteDoubleBigEndian(destination, value ?? Double.NaN);
    }

    public static ushort? ReadUInt16BigEndian(ReadOnlySpan<byte> source)
    {
        ushort value = BinaryPrimitives.ReadUInt16BigEndian(source);
        return value == Null.UInt16 ? null : value;
    }

    public static void WriteUInt16BigEndian(Span<byte> destination, ushort? value)
    {
        BinaryPrimitives.WriteUInt16BigEndian(destination, value ?? Null.UInt16);
    }

    public static short? ReadInt16BigEndian(ReadOnlySpan<byte> source)
    {
        short value = BinaryPrimitives.ReadInt16BigEndian(source);
        return value == Null.Int16 ? null : value;
    }

    public static void WriteInt16BigEndian(Span<byte> destination, short? value)
    {
        BinaryPrimitives.WriteInt16BigEndian(destination, value ?? Null.Int16);
    }

    public static uint? ReadUInt32BigEndian(ReadOnlySpan<byte> source)
    {
        uint value = BinaryPrimitives.ReadUInt32BigEndian(source);
        return value == Null.UInt32 ? null : value;
    }

    public static void WriteUInt32BigEndian(Span<byte> destination, uint? value)
    {
        BinaryPrimitives.WriteUInt32BigEndian(destination, value ?? Null.UInt32);
    }

    public static int? ReadInt32BigEndian(ReadOnlySpan<byte> source)
    {
        int value = BinaryPrimitives.ReadInt32BigEndian(source);
        return value == Null.Int32 ? null : value;
    }

    public static void WriteInt32BigEndian(Span<byte> destination, int? value)
    {
        BinaryPrimitives.WriteInt32BigEndian(destination, value ?? Null.Int32);
    }

    public static ulong? ReadUInt64BigEndian(ReadOnlySpan<byte> source)
    {
        ulong value = BinaryPrimitives.ReadUInt64BigEndian(source);
        return value == Null.UInt64 ? null : value;
    }

    public static void WriteUInt64BigEndian(Span<byte> destination, ulong? value)
    {
        BinaryPrimitives.WriteUInt64BigEndian(destination, value ?? Null.UInt64);
    }

    public static long? ReadInt64BigEndian(ReadOnlySpan<byte> source)
    {
        long value = BinaryPrimitives.ReadInt64BigEndian(source);
        return value == Null.Int64 ? null : value;
    }

    public static void WriteInt64BigEndian(Span<byte> destination, long? value)
    {
        BinaryPrimitives.WriteInt64BigEndian(destination, value ?? Null.Int64);
    }

    public static string? ReadString(ReadOnlySpan<byte> source)
    {
        if (source[0] == 0)
        {
            return null;
        }
        return Encoding.UTF8.GetString(source);
    }

    public static void WriteString(Span<byte> destination, string? value)
    {
        if (String.IsNullOrEmpty(value))
        {
            destination[0] = 0x0;
        }
        else
        {
            Encoding.UTF8.GetBytes(value, destination);
        }
    }
}
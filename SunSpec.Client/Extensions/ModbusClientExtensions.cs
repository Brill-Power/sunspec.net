/*
 * Copyright (c) 2025 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FluentModbus;

namespace SunSpec.Client.Extensions;

public static class ModbusClientExtensions
{
    public const int ModbusPageWidthInRegisters = 125;
    public const int ModbusPageWidthInBytes = ModbusPageWidthInRegisters * 2;

    private delegate Span<T> Read<T>(ModbusClient client, byte unitId, int startingRegister, int count);
    private delegate Task<Memory<T>> ReadAsync<T>(ModbusClient client, byte unitId, int startingRegister, int count);

    public static Span<T> ReadManyInputRegisters<T>(this ModbusClient self, byte unitId, int startingRegister, int count)
        where T : unmanaged
    {
        return self.ReadMany(unitId, startingRegister, count,
            static (mc, ui, sa, co) => mc.ReadInputRegisters<T>(ui, sa, co));
    }

    public static async Task<Memory<T>> ReadManyInputRegistersAsync<T>(this ModbusClient self, byte unitId, int startingRegister, int count)
        where T : unmanaged
    {
        return await self.ReadManyAsync(unitId, startingRegister, count,
            static (mc, ui, sa, co) => mc.ReadInputRegistersAsync<T>(ui, sa, co));
    }

    public static async Task ReadManyInputRegistersAsync<T>(this ModbusClient self, byte unitId, int startingRegister, int count, Memory<T> destination)
        where T : unmanaged
    {
        await self.ReadManyAsync(unitId, startingRegister, count,
            static (mc, ui, sa, co) => mc.ReadInputRegistersAsync<T>(ui, sa, co), destination);
    }

    public static Span<T> ReadManyHoldingRegisters<T>(this ModbusClient self, byte unitId, int startingRegister, int count)
        where T : unmanaged
    {
        return self.ReadMany(unitId, startingRegister, count,
            static (mc, ui, sa, co) => mc.ReadHoldingRegisters<T>(ui, sa, co));
    }

    public static void ReadManyHoldingRegisters<T>(this ModbusClient self, byte unitId, int startingRegister, int count, Span<T> destination)
        where T : unmanaged
    {
        self.ReadMany(unitId, startingRegister, count,
            static (mc, ui, sa, co) => mc.ReadHoldingRegisters<T>(ui, sa, co), destination);
    }

    public static async Task<Memory<T>> ReadManyHoldingRegistersAsync<T>(this ModbusClient self, byte unitId, int startingRegister, int count)
        where T : unmanaged
    {
        return await self.ReadManyAsync(unitId, startingRegister, count,
            static (mc, ui, sa, co) => mc.ReadHoldingRegistersAsync<T>(ui, sa, co));
    }

    public static async Task ReadManyHoldingRegistersAsync<T>(this ModbusClient self, byte unitId, int startingRegister, int count, Memory<T> destination)
        where T : unmanaged
    {
        await self.ReadManyAsync(unitId, startingRegister, count,
            static (mc, ui, sa, co) => mc.ReadHoldingRegistersAsync<T>(ui, sa, co), destination);
    }

    private static Span<T> ReadMany<T>(this ModbusClient self, byte unitId, int startingRegister, int count, Read<T> reader)
        where T : unmanaged
    {
        Span<T> result = new T[count];
        self.ReadMany(unitId, startingRegister, count, reader, result);
        return result;
    }

    private static void ReadMany<T>(this ModbusClient self, byte unitId, int startingRegister, int count, Read<T> reader, Span<T> destination)
        where T : unmanaged
    {
        int maxWidth = ConvertByteCountToWordCount<T>(ModbusPageWidthInBytes);
        for (int i = 0; i < count; i += maxWidth)
        {
            int width = Math.Min(maxWidth, count - i);
            ReadOnlySpan<T> values = reader(self, unitId, startingRegister + i, width);
            values.CopyTo(destination.Slice(i));
        }
    }

    private static async Task<Memory<T>> ReadManyAsync<T>(this ModbusClient self, byte unitId, int startingRegister, int count, ReadAsync<T> reader)
        where T : unmanaged
    {
        Memory<T> result = new T[count];
        await ReadManyAsync(self, unitId, startingRegister, count, reader, result);
        return result;
    }

    private static async Task ReadManyAsync<T>(this ModbusClient self, byte unitId, int startingRegister, int count, ReadAsync<T> reader, Memory<T> destination)
        where T : unmanaged
    {
        int maxWidth = ConvertByteCountToWordCount<T>(ModbusPageWidthInBytes);
        for (int i = 0; i < count; i += maxWidth)
        {
            int width = Math.Min(maxWidth, count - i);
            ReadOnlyMemory<T> values = await reader(self, unitId, startingRegister + i, width);
            values.CopyTo(destination.Slice(i));
        }
    }

    private static int ConvertByteCountToWordCount<T>(int count)
        where T : unmanaged
    {
        int size = typeof(T) == typeof(bool) ? 1 : Marshal.SizeOf<T>();
        return count / size;
    }
}

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
        return self.ReadMany(unitId, startingRegister, count, static (mc, ui, sa, co) => mc.ReadInputRegisters<T>(ui, sa, co));
    }

    public static async Task<Memory<T>> ReadManyInputRegistersAsync<T>(this ModbusClient self, byte unitId, int startingRegister, int count)
        where T : unmanaged
    {
        return await self.ReadManyAsync(unitId, startingRegister, count, static (mc, ui, sa, co) => mc.ReadInputRegistersAsync<T>(ui, sa, co));
    }

    public static Span<T> ReadManyHoldingRegisters<T>(this ModbusClient self, byte unitId, int startingRegister, int count)
        where T : unmanaged
    {
        return self.ReadMany(unitId, startingRegister, count, static (mc, ui, sa, co) => mc.ReadHoldingRegisters<T>(ui, sa, co));
    }

    public static async Task<Memory<T>> ReadManyHoldingRegistersAsync<T>(this ModbusClient self, byte unitId, int startingRegister, int count)
        where T : unmanaged
    {
        return await self.ReadManyAsync(unitId, startingRegister, count, static (mc, ui, sa, co) => mc.ReadHoldingRegistersAsync<T>(ui, sa, co));
    }

    private static Span<T> ReadMany<T>(this ModbusClient self, byte unitId, int startingRegister, int count, Read<T> reader)
        where T : unmanaged
    {
        int maxWidth = ConvertByteCountToWordCount<T>(ModbusPageWidthInBytes);
        Span<T> result = new T[count];
        for (int i = 0; i < count; i += maxWidth)
        {
            int width = Math.Min(maxWidth, count - i);
            ReadOnlySpan<T> values = reader(self, unitId, startingRegister + i, width);
            values.CopyTo(result.Slice(i));
        }
        return result;
    }

    private static async Task<Memory<T>> ReadManyAsync<T>(this ModbusClient self, byte unitId, int startingRegister, int count, ReadAsync<T> reader)
        where T : unmanaged
    {
        int maxWidth = ConvertByteCountToWordCount<T>(ModbusPageWidthInBytes);
        Memory<T> result = new T[count];
        for (int i = 0; i < count; i += maxWidth)
        {
            int width = Math.Min(maxWidth, count - i);
            ReadOnlyMemory<T> values = await reader(self, unitId, startingRegister + i, width);
            values.CopyTo(result.Slice(i));
        }
        return result;
    }

    private static int ConvertByteCountToWordCount<T>(int count)
        where T : unmanaged
    {
        int size = typeof(T) == typeof(bool) ? 1 : Marshal.SizeOf<T>();
        return count / size;
    }
}

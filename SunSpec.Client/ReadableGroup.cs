/*
 * Copyright (c) 2024 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using System;
using System.Threading.Tasks;
using FluentModbus;
using SunSpec.Models;

namespace SunSpec.Client;

public class ReadableGroup
{
    private readonly ModbusClient _modbusClient;
    private readonly ushort _startAddress;
    private readonly ushort _modelLength;

    public ReadableGroup(Group group, ModbusClient modbusClient, ushort startAddress, ushort modelLength)
    {
        _modbusClient = modbusClient;
        _startAddress = startAddress;
        _modelLength = modelLength;
        Group = group;
    }

    public Group Group { get; init; }

    public async Task ReadAsync()
    {
        Memory<byte> data = await _modbusClient.ReadHoldingRegistersAsync(SunSpecClient.DefaultUnitIdentifier, _startAddress, (ushort)(_modelLength + 1)); // + 1 for end inclusive.

        ushort left = 0;
        ushort right;
        foreach (Point point in Group.Points)
        {
            if (point.Type == PointType.Pad)
            {
                continue;
            }

            right = (ushort)(left + point.Size * 2); // * 2 because modbus is 16 bit, whereas bytes are 8 bit.
            ReadAndSetPointValue(point, data.Span, left, right);

            left = right;
        }
    }

    private static void ReadAndSetPointValue(Point point, ReadOnlySpan<byte> bytes, ushort left, ushort right)
    {
        ReadOnlySpan<byte> slice = bytes[left..right];
        point.Value = GetPointValue(point.Type, slice);
    }

    private static object? GetPointValue(PointType pointType, ReadOnlySpan<byte> slice)
        => pointType switch
        {
            PointType.UInt16 or PointType.Enum16 => ModbusBinaryConversion.ReadUShort(slice),
            PointType.SunSsf or PointType.Int16 => ModbusBinaryConversion.ReadShort(slice),
            PointType.Acc32 or PointType.Bitfield32 or PointType.UInt32 => ModbusBinaryConversion.ReadUInt(slice),
            PointType.String => ModbusBinaryConversion.ReadString(slice),
            _ => null,
        };
}
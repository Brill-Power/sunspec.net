/*
 * Copyright (c) 2024 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using System;
using System.Text;
using System.Threading.Tasks;
using FluentModbus;
using SunSpec.Client.Extensions;
using SunSpec.Models;
using SunSpec.Models.Generated;

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
        Memory<byte> data = await _modbusClient.ReadManyHoldingRegistersAsync<byte>(SunSpecClient.DefaultUnitIdentifier, _startAddress, (ushort)(_modelLength + 1) * 2); // + 1 for end inclusive.

        ushort position = 0;
        foreach (Point point in Group.Points)
        {
            if (point.Type == PointType.Pad)
            {
                continue;
            }

            point.Value = GetPointValue(point.Type, data.Span.Slice(position, point.Size * 2));
            position += (ushort)(point.Size * 2);
        }
    }

    private static object? GetPointValue(PointType pointType, ReadOnlySpan<byte> slice)
    {
        switch (pointType)
        {
            case PointType.UInt16:
            case PointType.Enum16:
                return SunSpecNullablePrimitives.ReadUInt16BigEndian(slice);
            case PointType.Int16:
            case PointType.SunSsf: // do we want this?
                return SunSpecNullablePrimitives.ReadInt16BigEndian(slice);
            case PointType.Acc32:
            case PointType.Bitfield32:
            case PointType.UInt32:
                return SunSpecNullablePrimitives.ReadUInt32BigEndian(slice);
            case PointType.Int32:
                return SunSpecNullablePrimitives.ReadInt32BigEndian(slice);
            case PointType.Int64:
                return SunSpecNullablePrimitives.ReadInt64BigEndian(slice);
            case PointType.UInt64:
                return SunSpecNullablePrimitives.ReadUInt64BigEndian(slice);
            case PointType.Float32:
                return SunSpecNullablePrimitives.ReadSingleBigEndian(slice);
            case PointType.Float64:
                return SunSpecNullablePrimitives.ReadDoubleBigEndian(slice);
            case PointType.String:
                return Encoding.UTF8.GetString(slice[0..^1]);
            default:
                return null;
        }
    }
}
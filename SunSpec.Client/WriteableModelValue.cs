/*
 * Copyright (c) 2024 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using System;
using System.Text;
using SunSpec.Models;
using SunSpec.Models.Generated;

namespace SunSpec.Client;

public class WriteableModelValue : ModelValue
{
    private readonly IModbusClient _client;
    private readonly ushort _startAddress;
    private readonly Memory<byte> _buffer;
    private readonly Func<object?, object?> _descaler;

    internal WriteableModelValue(IModbusClient client, ushort startAddress, Point point, Memory<byte> buffer, int offset,
        Func<object?, object?> scaler, Func<object?, object?> descaler) : base(point, buffer, offset, scaler)
    {
        _client = client;
        _startAddress = startAddress;
        _buffer = buffer;
        _descaler = descaler;
    }

    public override object? Value
    {
        get => base.Value;
        set
        {
            byte[] bytes = new byte[Point.Size * 2];
            Span<byte> span = bytes;
            value = _descaler(value);
            switch (Point.Type)
            {
                case PointType.Acc16:
                case PointType.Bitfield16:
                case PointType.UInt16:
                case PointType.Enum16:
                    SunSpecNullablePrimitives.WriteUInt16BigEndian(span, (ushort?)value);
                    break;
                case PointType.Int16:
                case PointType.SunSsf: // do we want this?
                    SunSpecNullablePrimitives.WriteInt16BigEndian(span, (short?)value);
                    break;
                case PointType.Acc32:
                case PointType.Bitfield32:
                case PointType.UInt32:
                case PointType.Enum32:
                    SunSpecNullablePrimitives.WriteUInt32BigEndian(span, (uint?)value);
                    break;
                case PointType.Int32:
                    SunSpecNullablePrimitives.WriteInt32BigEndian(span, (int?)value);
                    break;
                case PointType.Acc64:
                case PointType.Bitfield64:
                case PointType.UInt64:
                    SunSpecNullablePrimitives.WriteUInt64BigEndian(span, (ulong?)value);
                    break;
                case PointType.Int64:
                    SunSpecNullablePrimitives.WriteInt64BigEndian(span, (long?)value);
                    break;
                case PointType.Float32:
                    SunSpecNullablePrimitives.WriteSingleBigEndian(span, (float?)value);
                    break;
                case PointType.Float64:
                    SunSpecNullablePrimitives.WriteDoubleBigEndian(span, (double?)value);
                    break;
                case PointType.String:
                    Encoding.UTF8.GetBytes((string?)value, span);
                    break;
                default:
                    throw new NotSupportedException($"Value of type {Point.Type} is not currently supported.");
            }
            _client.WriteRegisters((ushort)(_startAddress + (_offset / 2)), bytes);
            // update buffer that backs in memory representation
            span.CopyTo(_buffer.Span.Slice(_offset));
        }
    }
}

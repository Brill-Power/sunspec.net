/*
 * Copyright (c) 2024 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using System;
using System.Collections.Generic;
using System.Text;
using SunSpec.Models;
using SunSpec.Models.Extensions;
using SunSpec.Models.Generated;

namespace SunSpec.Client;

public class ModelValue : IModelValue
{
    private readonly ReadOnlyMemory<byte> _buffer;
    protected readonly int _offset;

    internal ModelValue(Point point, ReadOnlyMemory<byte> buffer, int offset)
    {
        Point = point;
        _buffer = buffer;
        _offset = offset;
    }

    public Point Point { get; }

    public virtual object? Value
    {
        get
        {
            ReadOnlySpan<byte> slice = _buffer.Span.Slice(_offset, Point.Size * 2);
            object? value;
            switch (Point.Type)
            {
                case PointType.Acc16:
                case PointType.Bitfield16:
                case PointType.UInt16:
                case PointType.Enum16:
                    value = SunSpecNullablePrimitives.ReadUInt16BigEndian(slice);
                    break;
                case PointType.Int16:
                case PointType.SunSsf: // do we want this?
                    value = SunSpecNullablePrimitives.ReadInt16BigEndian(slice);
                    break;
                case PointType.Acc32:
                case PointType.Bitfield32:
                case PointType.UInt32:
                case PointType.Enum32:
                    value = SunSpecNullablePrimitives.ReadUInt32BigEndian(slice);
                    break;
                case PointType.Int32:
                    value = SunSpecNullablePrimitives.ReadInt32BigEndian(slice);
                    break;
                case PointType.Acc64:
                case PointType.Bitfield64:
                case PointType.UInt64:
                    value = SunSpecNullablePrimitives.ReadUInt64BigEndian(slice);
                    break;
                case PointType.Int64:
                    value = SunSpecNullablePrimitives.ReadInt64BigEndian(slice);
                    break;
                case PointType.Float32:
                    value = SunSpecNullablePrimitives.ReadSingleBigEndian(slice);
                    break;
                case PointType.Float64:
                    value = SunSpecNullablePrimitives.ReadDoubleBigEndian(slice);
                    break;
                case PointType.String:
                    value = Encoding.UTF8.GetString(slice);
                    break;
                default:
                    return null;
            }
            if (value is not null && Point.Type.IsEnumOrBitfield())
            {
                if (Point.Type.IsEnum())
                {
                    foreach (Symbol symbol in Point.Symbols)
                    {
                        if (symbol.Value.Equals(value))
                        {
                            return symbol.Name;
                        }
                    }
                }
                if (Point.Type.IsBitfield())
                {
                    List<string> values = [];
                    ulong t = (ulong)Convert.ChangeType(value, TypeCode.UInt64);
                    foreach (Symbol symbol in Point.Symbols)
                    {
                        if ((t & (ulong)(1 << symbol.Value)) == (ulong)(1 << symbol.Value))
                        {
                            values.Add(symbol.Name);
                        }
                    }
                    return values.ToArray();
                }
            }
            return value;
        }
        set
        {
            throw new InvalidOperationException($"Cannot set value of read-only point {Point.Name}.");
        }
    }
}

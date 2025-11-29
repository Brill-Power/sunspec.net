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
            ReadOnlySpan<byte> slice = _buffer.Span.Slice(_offset);
            switch (Point.Type)
            {
                case PointType.Acc16:
                case PointType.Bitfield16:
                case PointType.UInt16:
                case PointType.Enum16:
                    return SunSpecNullablePrimitives.ReadUInt16BigEndian(slice);
                case PointType.Int16:
                case PointType.SunSsf: // do we want this?
                    return SunSpecNullablePrimitives.ReadInt16BigEndian(slice);
                case PointType.Acc32:
                case PointType.Bitfield32:
                case PointType.UInt32:
                case PointType.Enum32:
                    return SunSpecNullablePrimitives.ReadUInt32BigEndian(slice);
                case PointType.Int32:
                    return SunSpecNullablePrimitives.ReadInt32BigEndian(slice);
                case PointType.Acc64:
                case PointType.Bitfield64:
                case PointType.UInt64:
                    return SunSpecNullablePrimitives.ReadUInt64BigEndian(slice);
                case PointType.Int64:
                    return SunSpecNullablePrimitives.ReadInt64BigEndian(slice);
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
        set
        {
            throw new InvalidOperationException($"Cannot set value of read-only point {Point.Name}.");
        }
    }
}

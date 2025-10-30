/*
 * Copyright (c) 2024 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
namespace SunSpec.Models.Generated;

public interface ISunSpecModel
{
    ushort ID { get; }
    ushort Length { get; }

    void NotifyValueChanged(int relativeRegisterId);
}

public interface ISunSpecModel<T> : ISunSpecModel
    where T : ISunSpecModel<T>
{
    static abstract T Create(Memory<byte> buffer);
}

/*
 * Copyright (c) 2024 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
namespace SunSpec.Models.Generated.Server;

public interface IServerModel
{
    ushort ID { get; }
    ushort Length { get; }
}

public interface IServerModel<T> : IServerModel
    where T : IServerModel<T>
{
    static abstract T Create(Memory<byte> buffer);
}
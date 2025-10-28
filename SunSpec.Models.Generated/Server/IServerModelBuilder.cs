/*
 * Copyright (c) 2024 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
namespace SunSpec.Models.Generated.Server;

public interface IServerModelBuilder
{
    bool Build(Memory<byte> buffer, out int length, out IServerModel model);
}

public interface IServerModelBuilder<T> : IServerModelBuilder
    where T : IServerModel<T>
{
    T Model { get; }
}
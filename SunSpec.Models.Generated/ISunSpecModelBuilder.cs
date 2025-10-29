/*
 * Copyright (c) 2024 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
namespace SunSpec.Models.Generated;

public interface ISunSpecModelBuilder
{
    bool Build(Memory<byte> buffer, out int length, out ISunSpecModel model);
}

public interface ISunSpecModelBuilder<T> : ISunSpecModelBuilder
    where T : ISunSpecModel<T>
{
    T Model { get; }
}
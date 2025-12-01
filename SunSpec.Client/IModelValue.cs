/*
 * Copyright (c) 2024 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using SunSpec.Models;

namespace SunSpec.Client;

public interface IModelValue
{
    Point Point { get; }
    object? Value { get; set; }
}

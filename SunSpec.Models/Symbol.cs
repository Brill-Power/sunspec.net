/*
 * Copyright (c) 2024 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
namespace SunSpec.Models;

public class Symbol : EntityBase
{
    public required string Name { get; set; }
    public required int Value { get; set; }
}
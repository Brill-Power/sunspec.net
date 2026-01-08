/*
 * Copyright (c) 2025 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using System.Collections.Generic;

namespace SunSpec.Server;

public readonly struct RegistersChangedEventArgs
{
    public RegistersChangedEventArgs(int[] registers)
    {
        Registers = registers;
    }

    public readonly IReadOnlyList<int> Registers { get; }
}
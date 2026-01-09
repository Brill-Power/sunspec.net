/*
 * Copyright (c) 2025 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using System;
using System.Threading.Tasks;

namespace SunSpec.Client;

public interface IModbusClient : IDisposable
{
    ValueTask<Memory<byte>> ReadHoldingRegistersAsync(int startingRegister, int count);
    ValueTask ReadHoldingRegistersAsync(int startingRegister, Memory<byte> destination);
    void WriteRegisters(int startingRegister, byte[] value);
}
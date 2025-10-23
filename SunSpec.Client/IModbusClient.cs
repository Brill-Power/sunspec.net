/*
 * Copyright (c) 2024 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using System;
using System.Threading.Tasks;

namespace SunSpec.Client;

public interface IModbusClient : IDisposable
{
    ValueTask ConnectAsync();

    void Connect();

    Task<byte[]> ReadHoldingRegisterAsync(uint address, uint count);
}
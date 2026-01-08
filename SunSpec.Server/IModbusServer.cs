/*
 * Copyright (c) 2025 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using System;
using System.Net;

namespace SunSpec.Server;

public interface IModbusServer : IDisposable
{
    event EventHandler<RegistersChangedEventArgs> RegistersChanged;

    void Start();
    void Start(IPEndPoint endpoint);
    void Stop();

    Memory<byte> GetHoldingRegisterMemory();
}

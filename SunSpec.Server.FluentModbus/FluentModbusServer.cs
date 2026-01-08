/*
 * Copyright (c) 2025 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using System;
using System.Net;
using BrillPower.FluentModbus;

namespace SunSpec.Server.FluentModbus;

public class FluentModbusServer : IModbusServer
{
    private const byte DefaultUnitIdentifier = 0x01;
    private const byte ZeroUnitIdentifier = 0x00;

    private readonly ModbusTcpServer _server;
    private readonly byte _unitId;

    public FluentModbusServer() : this(new ModbusTcpServer())
    {
    }

    public FluentModbusServer(ModbusTcpServer server, byte unitId = DefaultUnitIdentifier)
    {
        _unitId = unitId;

        _server = server;
        _server.EnableRaisingEvents = true;
        _server.RegistersChanged += OnRegistersChanged;
        _server.ConnectionTimeout = TimeSpan.MaxValue; // 1 minute timeout generally troublesome
        if (unitId != ZeroUnitIdentifier)
        {
            _server.AddUnit(unitId);
        }
    }

    public event EventHandler<RegistersChangedEventArgs>? RegistersChanged;

    public void Dispose()
    {
        _server.Dispose();
    }

    public Memory<byte> GetHoldingRegisterMemory()
    {
        return _server.GetHoldingRegisterMemory(_unitId);
    }

    public void Start()
    {
        _server.Start();
    }

    public void Start(IPEndPoint endpoint)
    {
        _server.Start(endpoint);
    }

    public void Stop()
    {
        _server.Stop();
    }

    private void OnRegistersChanged(object? sender, BrillPower.FluentModbus.RegistersChangedEventArgs e)
    {
        RegistersChanged?.Invoke(this, new RegistersChangedEventArgs(e.Registers));
    }
}
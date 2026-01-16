/*
 * Copyright (c) 2025 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using System;
using System.Threading.Tasks;
using BrillPower.FluentModbus;
using SunSpec.Client.FluentModbus.Extensions;

namespace SunSpec.Client.FluentModbus;

public class FluentModbusClient : IModbusClient
{
    private const byte DefaultUnitIdentifier = 0x01;

    private readonly ModbusClient _client;
    private readonly byte _unitId;

    public FluentModbusClient(ModbusClient client, byte unitId = DefaultUnitIdentifier)
    {
        _client = client;
        _unitId = unitId;
    }

    public void Dispose()
    {
        if (_client is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    public async ValueTask ReadHoldingRegistersAsync(int startingRegister, Memory<byte> destination)
    {
        await _client.ReadManyHoldingRegistersAsync(_unitId, startingRegister, destination);
    }

    public async ValueTask<Memory<byte>> ReadHoldingRegistersAsync(int startingRegister, int count)
    {
        return await _client.ReadManyHoldingRegistersAsync(_unitId, startingRegister, count * 2);
    }

    public void WriteRegisters(int startingRegister, byte[] value)
    {
        _client.WriteMultipleRegisters(_unitId, (ushort)startingRegister, value);
    }
}
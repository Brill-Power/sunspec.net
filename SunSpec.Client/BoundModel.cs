/*
 * Copyright (c) 2024 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrillPower.FluentModbus;
using SunSpec.Client.Extensions;
using SunSpec.Models;

namespace SunSpec.Client;

public class BoundModel
{
    private readonly ModbusClient _modbusClient;
    private readonly byte _unitId;
    private readonly Memory<byte> _buffer;
    private readonly ushort _startAddress;
    private readonly ushort _modelLength;

    public BoundModel(Model model, ModbusClient client, byte unitId, ushort startAddress, ushort modelLength)
    {
        _modbusClient = client;
        _unitId = unitId;
        _startAddress = startAddress;
        _modelLength = modelLength;
        Model = model;

        _buffer = new byte[modelLength * 2];
        List<IModelValue> values = new List<IModelValue>();
        int offset = 0; // local offset in bytes within buffer
        foreach (Point point in model.Group.Points)
        {
            IModelValue modelValue;
            if (point.IsReadWrite)
            {
                modelValue = new WriteableModelValue(client, unitId, startAddress, point, _buffer, offset);
            }
            else
            {
                modelValue = new ModelValue(point, _buffer, offset);
            }
            values.Add(modelValue);
            offset += point.Size * 2;
        }
        Values = values.AsReadOnly();
    }

    public Model Model { get; init; }

    public IReadOnlyList<IModelValue> Values { get; }

    public async Task ReadAsync()
    {
        await _modbusClient.ReadManyHoldingRegistersAsync(_unitId, _startAddress, _modelLength * 2, _buffer);
    }
}
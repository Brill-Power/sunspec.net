/*
 * Copyright (c) 2024 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SunSpec.Models;

namespace SunSpec.Client;

public class BoundModel
{
    private readonly IModbusClient _modbusClient;
    private readonly Memory<byte> _buffer;
    private readonly ushort _startAddress;
    private readonly Dictionary<string, IModelValue> _scaleFactorsByName = [];

    public BoundModel(Model model, IModbusClient client, ushort startAddress, ushort modelLength)
    {
        _modbusClient = client;
        _startAddress = startAddress;
        Model = model;

        _buffer = new byte[modelLength * 2];
        List<IModelValue> values = new List<IModelValue>();
        int offset = 0; // local offset in bytes within buffer
        foreach (Point point in model.Group.Points)
        {
            Func<object?, object?> scaler = x => x;
            Func<object?, object?> descaler = x => x;
            if (!String.IsNullOrEmpty(point.ScaleFactor))
            {
                scaler = x => Scale(x, point.ScaleFactor);
                descaler = x => Descale(x, point.ScaleFactor);
            }
            IModelValue modelValue;
            if (point.IsReadWrite)
            {
                modelValue = new WriteableModelValue(client, startAddress, point, _buffer, offset, scaler, descaler);
            }
            else
            {
                modelValue = new ModelValue(point, _buffer, offset, scaler);
            }
            values.Add(modelValue);
            if (point.Type == PointType.SunSsf)
            {
                _scaleFactorsByName.Add(point.Name, modelValue);
            }
            offset += point.Size * 2;
        }
        Values = values.AsReadOnly();
    }

    public Model Model { get; init; }

    public IReadOnlyList<IModelValue> Values { get; }

    public async Task ReadAsync()
    {
        await _modbusClient.ReadHoldingRegistersAsync(_startAddress, _buffer);
    }

    private object? Scale(object? value, string sfName)
    {
        double? d = (double?)Convert.ChangeType(value, TypeCode.Double);
        if (d.HasValue && _scaleFactorsByName.TryGetValue(sfName, out IModelValue? scaleFactor) &&
            scaleFactor.Value is short s)
        {
            return d.Value * Math.Pow(10, s);
        }
        return value;
    }

    private object? Descale(object? value, string sfName)
    {
        double? d = (double?)Convert.ChangeType(value, TypeCode.Double);
        if (d.HasValue && _scaleFactorsByName.TryGetValue(sfName, out IModelValue? scaleFactor) &&
            scaleFactor.Value is short s)
        {
            return d.Value / Math.Pow(10, s);
        }
        return value;
    }
}

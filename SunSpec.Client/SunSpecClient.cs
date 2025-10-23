/*
 * Copyright (c) 2024 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentModbus;
using SunSpec.Models;

namespace SunSpec.Client;

public class SunSpecClient : IDisposable
{
    internal const byte DefaultUnitIdentifier = 0x00;

    private const int CommonModelId = 1;
    private const ushort CommonModelStartAddress = 2;
    private const int CommonModelStartByte = 4;
    private const ushort ModelIdAndLength = 2;

    private readonly ModbusClient _client;
    private readonly Dictionary<uint, ReadableGroup> _models;

    public SunSpecClient(ModbusClient client)
    {
        _client = client;
        _models = [];
    }

    public void Dispose()
    {
        if (_client is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    public async Task ScanAsync()
    {
        Memory<byte> data = await _client.ReadHoldingRegistersAsync(DefaultUnitIdentifier, 0, 4); // SunS + modelId + modelLength

        EnsureStartsWithSunSpecPreamble(data.Span);
        ushort commonModelLength = ProcessCommonModel(data.Span);

        ushort readFrom = (ushort)(CommonModelStartAddress + commonModelLength);
        Memory<byte> buffer = await _client.ReadHoldingRegistersAsync(DefaultUnitIdentifier, readFrom, ModelIdAndLength);

        while (!IsSunSpecEndModelId(buffer.Span))
        {
            (ushort modelId, ushort modelLength) = GetModelIdAndLength(buffer.Span);

            if (modelId == 0)
            {
                readFrom += 2;
            }
            else
            {
                Model model = Model.GetModel(modelId);
                _models.Add(modelId, new ReadableGroup(model.Group, _client, readFrom, modelLength));
                readFrom += modelLength;
            }

            buffer = await _client.ReadHoldingRegistersAsync(DefaultUnitIdentifier, readFrom, ModelIdAndLength);
        }
    }

    public ReadableGroup? Common { get; private set; }

    public IReadOnlyDictionary<uint, ReadableGroup> Models { get => _models; }

    private static void EnsureStartsWithSunSpecPreamble(ReadOnlySpan<byte> data)
    {
        if (data.Length < 4 || Encoding.ASCII.GetString(data[0..4]) != "SunS")
        {
            throw new SunSpecException("Device is not a SunSpec device.");
        }
    }

    private static bool IsSunSpecEndModelId(ReadOnlySpan<byte> buffer) => buffer[0] == 255 && buffer[1] == 255;

    private static (ushort, ushort) GetModelIdAndLength(ReadOnlySpan<byte> bytes)
    {
        ushort modelId = ModbusBinaryConversion.ReadUShort(bytes[..2]);
        ushort modelLength = ModbusBinaryConversion.ReadUShort(bytes[2..4]);
        return (modelId, modelLength);
    }

    private ushort ProcessCommonModel(ReadOnlySpan<byte> data)
    {
        Model commonModel = Model.GetModel(CommonModelId);

        ushort modelId = ModbusBinaryConversion.ReadUShort(data.Slice(CommonModelStartByte, 2));

        if (modelId != CommonModelId)
        {
            throw new SunSpecException("Device does not start with the SunSpec common model.");
        }

        ushort commonModelLength = ModbusBinaryConversion.ReadUShort(data.Slice(CommonModelStartByte + 2, 2));

        Common = new ReadableGroup(commonModel.Group, _client, CommonModelStartAddress, commonModelLength);
        return commonModelLength;
    }
}
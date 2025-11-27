/*
 * Copyright (c) 2024 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentModbus;
using SunSpec.Client.Extensions;
using SunSpec.Models;
using SunSpec.Models.Generated;

namespace SunSpec.Client;

public class SunSpecClient : IDisposable
{
    internal const byte DefaultUnitIdentifier = 0x01;

    private const int CommonModelId = 1;
    private const ushort CommonModelStartAddress = 2;
    private const int CommonModelStartByte = 4;
    private const ushort ModelIdAndLength = 2;

    private readonly ModbusClient _client;
    private readonly byte _unitId;
    private readonly Dictionary<uint, ReadableGroup> _schemata = [];
    private readonly List<ISunSpecModel> _models = [];

    public SunSpecClient(ModbusClient client, byte unitId = DefaultUnitIdentifier)
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

    public async Task ScanAsync()
    {
        _schemata.Clear();
        _models.Clear();

        Memory<byte> buffer = await _client.ReadHoldingRegistersAsync(_unitId, 0, 4); // SunS + modelId + modelLength

        EnsureStartsWithSunSpecPreamble(buffer.Span);
        EnsureCommonModel(buffer.Span);

        ushort readFrom = CommonModelStartAddress;
        do
        {
            buffer = await _client.ReadHoldingRegistersAsync(_unitId, readFrom, ModelIdAndLength);
            (ushort modelId, ushort modelLength) = GetModelIdAndLength(buffer.Span);

            if (modelId == 0)
            {
                readFrom += 2;
            }
            else
            {
                modelLength += 2;
                Model schema = Model.GetModel(modelId);
                if (!_schemata.ContainsKey(modelId))
                {
                    // TODO: think about whether we still want to provide readable access through these
                    // schemas; for now, just add the first one
                    _schemata.Add(modelId, new ReadableGroup(schema.Group, _client, readFrom, modelLength));
                }
                buffer = await _client.ReadManyHoldingRegistersAsync<byte>(_unitId, readFrom, modelLength * 2);
                ISunSpecModel model = SunSpecAnyModelBuilder.Create(modelId, buffer);
                _models.Add(model);
                readFrom += modelLength;
            }

            buffer = await _client.ReadHoldingRegistersAsync(_unitId, readFrom, ModelIdAndLength);
        }
        while (!IsSunSpecEndModelId(buffer.Span));
    }

    public ReadableGroup? Common => _schemata.TryGetValue(1, out ReadableGroup? group) ? group : null;

    public IReadOnlyList<ISunSpecModel> Models => _models;

    public IReadOnlyDictionary<uint, ReadableGroup> Schemata => _schemata;

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
        ushort modelId = BinaryPrimitives.ReadUInt16BigEndian(bytes[..2]);
        ushort modelLength = BinaryPrimitives.ReadUInt16BigEndian(bytes[2..4]);
        return (modelId, modelLength);
    }

    private static void EnsureCommonModel(ReadOnlySpan<byte> data)
    {
        ushort modelId = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(CommonModelStartByte, 2));
        if (modelId != CommonModelId)
        {
            throw new SunSpecException("Device does not start with the SunSpec common model.");
        }
    }
}
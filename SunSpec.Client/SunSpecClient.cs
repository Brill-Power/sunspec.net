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
    private readonly Dictionary<uint, IReadOnlyList<BoundModel>> _boundModelsById = [];
    private readonly List<BoundModel> _boundModels = [];
    private readonly List<ISunSpecModel> _proxies = [];

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
        PrepareScan();

        Dictionary<uint, List<BoundModel>> schemataByModel = [];

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
                Model model = Model.GetModel(modelId);
                if (!schemataByModel.TryGetValue(modelId, out List<BoundModel>? groups))
                {
                    _boundModelsById[modelId] = groups = [];
                }
                BoundModel boundModel = new BoundModel(model, _client, _unitId, readFrom, modelLength);
                groups.Add(boundModel);
                _boundModels.Add(boundModel);

                buffer = await _client.ReadManyHoldingRegistersAsync<byte>(_unitId, readFrom, modelLength * 2);
                ISunSpecModel typedProxy = SunSpecAnyModelBuilder.Create(modelId, buffer);
                _proxies.Add(typedProxy);
                readFrom += modelLength;
            }

            buffer = await _client.ReadHoldingRegistersAsync(_unitId, readFrom, ModelIdAndLength);
        }
        while (!IsSunSpecEndModelId(buffer.Span));

        foreach (uint key in schemataByModel.Keys)
        {
            _boundModelsById.Add(key, schemataByModel[key].AsReadOnly());
        }
    }

    private void PrepareScan()
    {
        _boundModelsById.Clear();
        _boundModels.Clear();
        _proxies.Clear();
    }

    public BoundModel? Common => _boundModelsById.TryGetValue(1, out IReadOnlyList<BoundModel>? groups) ? groups[0] : null;

    public IReadOnlyList<ISunSpecModel> Proxies => _proxies;

    public IReadOnlyList<BoundModel> BoundModels => _boundModels;

    public IReadOnlyDictionary<uint, IReadOnlyList<BoundModel>> BoundModelsByID => _boundModelsById;

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
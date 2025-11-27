/*
 * Copyright (c) 2024 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using FluentModbus;
using SunSpec.Models.Generated;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading;

namespace SunSpec.Server;

public class SunSpecServer : IDisposable
{
    private const byte ZeroUnitIdentifier = 0x00;
    private const byte DefaultUnitIdentifier = 0x01;

    private static readonly byte[] Preamble = Encoding.UTF8.GetBytes("SunS");

    private readonly byte _unitId;

    private readonly ILogger _logger;

    private readonly ModbusTcpServer _server;
    private readonly List<ISunSpecModelBuilder> _builders = [];
    private readonly CommonBuilder _commonModelBuilder = new CommonBuilder();
    private readonly SortedDictionary<int, ISunSpecModel> _modelsByStartingRegister = [];
    private int _currentRegister;

    public SunSpecServer(byte unitId = DefaultUnitIdentifier) : this(null, unitId)
    {
    }

    public SunSpecServer(ILoggerFactory? loggerFactory, byte unitId = DefaultUnitIdentifier)
    {
        _unitId = unitId;
        _logger = (ILogger?)loggerFactory?.CreateLogger<SunSpecServer>() ?? NullLogger.Instance;

        _server = new ModbusTcpServer((ILogger?)loggerFactory?.CreateLogger<ModbusTcpServer>() ?? NullLogger.Instance);
        _server.EnableRaisingEvents = true;
        _server.RegistersChanged += OnRegistersChanged;
        _server.ConnectionTimeout = TimeSpan.MaxValue; // 1 minute timeout generally troublesome
        if (unitId != ZeroUnitIdentifier)
        {
            _server.AddUnit(unitId);
        }

        Initialise();
        Build();
    }

    public Common? CommonModel => _commonModelBuilder.Model;

    public SunSpecServer RegisterModelBuilder(ISunSpecModelBuilder builder)
    {
        lock (_builders)
        {
            _builders.Add(builder);
        }
        return this;
    }

    public void Build()
    {
        if (_currentRegister != Preamble.Length / 2)
        {
            throw new InvalidOperationException($"Cannot call {nameof(Build)} unless {nameof(Initialise)} is called first.");
        }
        Memory<byte> holdingRegisters = _server.GetHoldingRegisterMemory(_unitId);
        lock (_builders)
        {
            foreach (ISunSpecModelBuilder builder in _builders)
            {
                if (builder.Build(holdingRegisters.Slice(_currentRegister * 2), out int length, out ISunSpecModel model))
                {
                    _modelsByStartingRegister.Add(_currentRegister, model);
                    _logger?.LogInformation($"Registered model {model.GetType().Name} (ID {model.ID}) of length {model.Length} at register {_currentRegister}.");
                    _currentRegister += length;
                }
            }
        }
        UpdateFooter();
    }

    public void Initialise()
    {
        lock (_builders)
        {
            _builders.Clear();
            _modelsByStartingRegister.Clear();
        }
        Span<byte> holdingRegisters = _server.GetHoldingRegisterBuffer(_unitId);
        holdingRegisters.Fill(0);
        Preamble.CopyTo(holdingRegisters);
        _currentRegister = Preamble.Length / 2;
        RegisterModelBuilder(_commonModelBuilder);
        UpdateFooter();
    }

    public void Start()
    {
        _server.Start();
    }

    public void Start(IPEndPoint endpoint)
    {
        _server.Start(endpoint);
        _logger?.LogInformation($"Started SunSpec server on {endpoint}");
    }

    public void Stop()
    {
        _server.Stop();
        _logger?.LogInformation("Stopping SunSpec server");
    }

    public void Dispose()
    {
        _server.Dispose();
    }

    private void UpdateFooter()
    {
        BinaryPrimitives.WriteUInt16BigEndian(_server.GetHoldingRegisterBuffer(_unitId).Slice(_currentRegister * 2), 0xFFFF);
    }

    private void OnRegistersChanged(object? sender, RegistersChangedEventArgs e)
    {
        foreach (int register in e.Registers)
        {
            foreach (int startingRegister in _modelsByStartingRegister.Keys.Reverse())
            {
                if (startingRegister < register)
                {
                    ISunSpecModel model = _modelsByStartingRegister[startingRegister];
                    int localRegister = register - startingRegister - 1;
                    _logger?.LogInformation($"Notifying model {model.GetType().Name} (ID {model.ID}) of change of register {localRegister}");
                    model.NotifyValueChanged(localRegister);
                    break;
                }
            }
        }
    }
}
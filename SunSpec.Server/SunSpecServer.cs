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
using SunSpec.Models.Generated.Server;

namespace SunSpec.Server;

public class SunSpecServer : IDisposable
{
    private static readonly byte[] Preamble = Encoding.UTF8.GetBytes("SunS");

    private readonly ModbusTcpServer _server;
    private readonly List<IServerModelBuilder> _builders = [];
    private readonly CommonBuilder _commonModelBuilder = new CommonBuilder();
    private readonly SortedDictionary<int, IServerModel> _modelsByStartingRegister = [];
    private int _currentRegister;

    public SunSpecServer()
    {
        _server = new ModbusTcpServer();
        _server.EnableRaisingEvents = true;
        _server.RegistersChanged += OnRegistersChanged;

        Initialise();
        Build();
    }

    public Common? CommonModel => _commonModelBuilder.Model;

    public SunSpecServer RegisterModelBuilder(IServerModelBuilder builder)
    {
        _builders.Add(builder);
        return this;
    }

    public void Build()
    {
        if (_currentRegister != Preamble.Length)
        {
            throw new InvalidOperationException($"Cannot call {nameof(Build)} unless {nameof(Initialise)} is called first.");
        }
        Memory<byte> holdingRegisters = _server.GetHoldingRegisterMemory();
        foreach (IServerModelBuilder builder in _builders)
        {
            if (builder.Build(holdingRegisters.Slice(_currentRegister * 2), out int length, out IServerModel model))
            {
                _modelsByStartingRegister.Add(_currentRegister, model);
                _currentRegister += length;
            }
        }
        UpdateFooter();
    }

    public void Initialise()
    {
        _builders.Clear();
        _modelsByStartingRegister.Clear();
        Span<byte> holdingRegisters = _server.GetHoldingRegisterBuffer();
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
    }

    public void Stop()
    {
        _server.Stop();
    }

    public void Dispose()
    {
        _server.Dispose();
    }

    private void UpdateFooter()
    {
        BinaryPrimitives.WriteUInt16BigEndian(_server.GetHoldingRegisterBuffer().Slice(_currentRegister * 2), 0xFFFF);
    }

    private void OnRegistersChanged(object? sender, RegistersChangedEventArgs e)
    {
        foreach (int register in e.Registers)
        {
            foreach (int startingRegister in _modelsByStartingRegister.Keys.Reverse())
            {
                if (startingRegister < register)
                {
                    IServerModel model = _modelsByStartingRegister[startingRegister];
                    model.NotifyValueChanged(register - startingRegister);
                }
            }
        }
    }
}
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Net;
using System.Text;
using FluentModbus;
using SunSpec.Models.Generated.Server;

namespace SunSpec.Server;

public class SunSpecServer : IDisposable
{
    private static readonly byte[] Preamble = Encoding.UTF8.GetBytes("SunS");

    private readonly ModbusTcpServer _server;
    private readonly List<IServerModelBuilder> _builders = new List<IServerModelBuilder>();
    private readonly CommonBuilder _commonModelBuilder = new CommonBuilder();
    private int _cursor;

    public SunSpecServer()
    {
        _server = new ModbusTcpServer();

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
        if (_cursor != Preamble.Length)
        {
            throw new InvalidOperationException($"Cannot call {nameof(Build)} unless {nameof(Initialise)} is called first.");
        }
        Memory<byte> holdingRegisters = _server.GetHoldingRegisterMemory();
        foreach (IServerModelBuilder builder in _builders)
        {
            _cursor += builder.Build(holdingRegisters.Slice(_cursor)) * 2;
        }
        UpdateFooter();
    }

    public void Initialise()
    {
        _builders.Clear();
        Span<byte> holdingRegisters = _server.GetHoldingRegisterBuffer();
        Preamble.CopyTo(holdingRegisters);
        _cursor = Preamble.Length;
        RegisterModelBuilder(_commonModelBuilder);
        UpdateFooter();
    }

    private void UpdateFooter()
    {
        BinaryPrimitives.WriteUInt16BigEndian(_server.GetHoldingRegisterBuffer().Slice(_cursor), 0xFFFF);
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
}
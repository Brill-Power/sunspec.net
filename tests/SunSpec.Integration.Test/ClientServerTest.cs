/*
 * Copyright (c) 2024-2025 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using FluentModbus;
using SunSpec.Client;
using SunSpec.Server;
using SunSpec.Models.Generated;
using System.Linq;
using System.Net;

namespace SunSpec.Integration.Test;

public class ClientServerTest
{
    [Fact]
    public async void Test()
    {
        SunSpecServer server = new SunSpecServer();
        server.Initialise();
        server.RegisterModelBuilder(new BatteryBuilder());
        LithiumIonBankBuilder bankBuilder = new LithiumIonBankBuilder();
        server.RegisterModelBuilder(bankBuilder);
        bankBuilder.AddLithiumIonBankString();
        LithiumIonStringBuilder stringBuilder = new LithiumIonStringBuilder();
        server.RegisterModelBuilder(stringBuilder);
        stringBuilder.AddLithiumIonStringModule();
        server.Build();
        server.CommonModel!.Manufacturer = "Brill Power";
        server.Start(new IPEndPoint(IPAddress.Loopback, 1502));

        // test some non-mandatory values
        Assert.Null(bankBuilder.Model.MinStringCurrent);
        Assert.Null(bankBuilder.Model.MaxStringCurrent);

        bankBuilder.Model.ScaleFactors.V = 0.01;
        bankBuilder.Model.ScaleFactors.CellV = 0.001;
        bankBuilder.Model.AverageStringVoltage = 24.0;
        bankBuilder.Model.LithiumIonBankStrings[0].AverageCellVoltage = 3.25;

        ModbusTcpClient tcpClient = new ModbusTcpClient();
        tcpClient.Connect(new IPEndPoint(IPAddress.Loopback, 1502), ModbusEndianness.BigEndian);
        SunSpecClient client = new SunSpecClient(tcpClient);
        await client.ScanAsync();
        Assert.Equal(4, client.Proxies.Count);
        LithiumIonBank? lithiumIonBank = client.Proxies.OfType<LithiumIonBank>().FirstOrDefault();
        Assert.NotNull(lithiumIonBank);
        Assert.Null(lithiumIonBank.MinStringCurrent);
        Assert.Null(lithiumIonBank.MaxStringCurrent);
        Assert.Equal(24.0, lithiumIonBank.AverageStringVoltage);
        Assert.Equal(1, lithiumIonBank.StringCount);
        LithiumIonBankString bankString = lithiumIonBank.LithiumIonBankStrings[0];
        Assert.Equal(3.25, bankString.AverageCellVoltage);
    }
}
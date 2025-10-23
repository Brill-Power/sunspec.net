using FluentModbus;
using SunSpec.Client;
using SunSpec.Server;
using SunSpec.Models.Generated.Server;

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
        server.CommonModel.Manufacturer = "Brill Power";
        server.Start();

        ModbusTcpClient tcpClient = new ModbusTcpClient();
        tcpClient.Connect(ModbusEndianness.BigEndian);
        SunSpecClient client = new SunSpecClient(tcpClient);
        await client.ScanAsync();
    }
}
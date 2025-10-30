# SunSpec.NET

This repository implements the SunSpec modbus specification in .NET. It is inspired by [pysunspec2](https://github.com/sunspec/pysunspec2).

## Getting Started

There is a rudimentary unit test in `SunSpec.Integration.Test` which gives a brief idea of how the library is intended to be used.

### Server

The server currently assumes Modbus TCP. There is no particular reason for this to remain the case, as the underlying Modbus library it uses supports both TCP and serial.

Create a server and initialise it:

```csharp
SunSpecServer server = new SunSpecServer();
server.Initialise();
```

This will add the 'Common' model that is, er, common to all SunSpec devices.

Add further models by calling `RegisterModelBuilder`:

```csharp
LithiumIonBankBuilder bankBuilder = new LithiumIonBankBuilder();
server.RegisterModelBuilder(bankBuilder);
```

Some models contain repeating groups: they can be added by methods on the model builders themselves:

```csharp
bankBuilder.AddLithiumIonBankString();
```

When you have composed your model builders, call `Build` and then `Start`:

```csharp
server.Build();

server.Start();
```

Once built, each builder's `Model` property will be populated with the constructed model. Set values on this
for them to be retrieved by clients.

Writable values have an event associated with them. Subscribe to the event to be notified when a client modifies
that value:

```csharp
DERCtlACBuilder derAcControlsBuilder;
...
derAcControlsBuilder.Model.ActivePowerSetpointWChanged += OnActivePowerSetpointWChanged;

private void OnActivePowerSetpointWChanged(object? sender, EventArgs e)
{
    // do something in response to changed value here
}
```

### Client

The client is less mature than the server; so far, its main purpose is to provide a counterpart to the server in
unit tests.

Create a `ModbusClient` (TCP or serial) and connect it as appropriate.

```csharp
ModbusTcpClient tcpClient = new ModbusTcpClient();
tcpClient.Connect(new IPEndPoint(IPAddress.Loopback, 1502), ModbusEndianness.BigEndian);
```

Create a `SunSpecClient` and pass the constructed `ModbusClient` to it, then call `ScanAsync`.

```csharp
SunSpecClient client = new SunSpecClient(tcpClient);
await client.ScanAsync();
```

This will discover the models available on the server and proxies constructed for them so that their values
can be read. Those proxies can be accessed from the `Models` property:

```csharp
LithiumIonBank? lithiumIonBank = client.Models.OfType<LithiumIonBank>().FirstOrDefault();
Console.WriteLine($"Voltage is {lithiumIonBank.AverageStringVoltage}");
```

## Model Generation

The model generator is quite comprehensive now. It supports:

* repeating groups, including where their counts are specified in a given register
* automatic handling of scale factors (accessible from the `ScaleFactors` property on a given model)

At present, mainly for aesthetic reasons, property names are derived from the SunSpec label field rather than
the name field. A reasonable enhancement would be to make this configurable.

## Dependencies

[FluentModbus](https://github.com/Brill-Power/FluentModbus) (fork)

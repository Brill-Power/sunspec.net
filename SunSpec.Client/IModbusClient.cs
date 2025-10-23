using System;
using System.Threading.Tasks;

namespace SunSpec.Client;
public interface IModbusClient : IDisposable
{
    ValueTask ConnectAsync();

    void Connect();

    Task<byte[]> ReadHoldingRegisterAsync(uint address, uint count);
}
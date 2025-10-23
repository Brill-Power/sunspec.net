# SunSpec.NET

This repository implements the SunSpec modbus specification in .NET. It is inspired by [pysunspec2](https://github.com/sunspec/pysunspec2).

## Background

Modbus is a client/server communication protocol that is commonly used on electronic devices. The data model consists of four tables

| Table | Access | Size | Description |
|-------|--------|------|-------------|
| Coil | Read/Write | 1 bit | Boolean data |
| Discrete input | Read | 1 bit | Boolean data |
| Input register | Read | 16 bit| Measurements and statuses |
| Holding register | Read/Write | 16 bit | Measurements and statuses |

SunSpec is a framework for storing PV/battery storage data using a modbus server. Data is only stored in the holding register and uses a series of well-defined or custom models for retrieving complex data.

In a SunSpec device, the first two addresses are `Su` and `nS`. Then there is a common model that contains information about the device and manufacturer. There can then be any of number of models containing data from the device. After the last model, the final register contains the SunSpec closing data `65535`.

To read data from a SunSpec device, you first "scan" the device to discover what models it uses, then read the desired model.

## Testing/running locally

If you do not have a modbus device, there several emulators to choose from. The integration tests use [one that can run in a Docker container](https://github.com/paulorb/modbus-simulator-cli), but it is not very popular so worth keeping an eye on alternatives.

[ModbusTool](https://github.com/ClassicDIY/ModbusTool) is very easy to install on windows but not WSL. It is useful for debugging the integration tests as it can communicate with the docker container. It can also run a client and server separately.

[ModbusTools](https://github.com/serhmarch/ModbusTools) is a little buggy but fairly easily to install on WSL.

## Dependencies

[FluentModbus](https://github.com/Brill-Power/FluentModbus) (fork)

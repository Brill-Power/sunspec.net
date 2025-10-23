using System;

namespace SunSpec.Client;

public class SunSpecException : Exception
{
    public SunSpecException(string? message) : base(message)
    {
    }

    public SunSpecException(string? message, Exception innerException) : base(message, innerException)
    {
    }
}
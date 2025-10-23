namespace SunSpec.Models;

public class Symbol : EntityBase
{
    public required string Name { get; set; }
    public required object? Value { get; set; }
}
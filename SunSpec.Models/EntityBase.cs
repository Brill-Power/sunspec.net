using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SunSpec.Models;

public abstract class EntityBase
{
    public string? Label { get; set; }
    [JsonPropertyName("desc")]
    public string? Description { get; set; }
    public string? Detail { get; set; }
    public string? Notes { get; set; }
    public List<string> Comments { get; set; } = new List<string>();
}
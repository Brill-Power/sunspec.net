/*
 * Copyright (c) 2024 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
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

    public class SunSpecReferenceOrCountConverter : JsonConverter<object?>
    {
        public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number && reader.TryGetUInt16(out ushort count))
            {
                // is a count
                return count;
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                return reader.GetString();
            }

            throw new InvalidDataException($"Unable to convert data at position {reader.Position} to the desired type.");
        }

        public override void Write(Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
        {
            if (value is string s)
            {
                writer.WriteStringValue(s);
            }
            if (value is not null)
            {
                int i = Convert.ToInt32(value);
                writer.WriteNumberValue(i);
            }
        }
}
}
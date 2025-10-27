/*
 * Copyright (c) 2024-2025 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SunSpec.Models;

public class Point : EntityBase
{
    public required string Name { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required PointType Type { get; set; }
    public required ushort Size { get; set; }
    public object? Value { get; set; }
    public int? Count { get; set; }
    [JsonPropertyName("sf")]
    public string? ScaleFactor { get; set; }
    public string? Units { get; set; }
    [JsonConverter(typeof(SunSpecIsReadOnlyConverter))]
    [JsonPropertyName("access")]
    public bool? IsReadOnly { get; set; }
    [JsonConverter(typeof(SunSpecMandatoryConverter))]
    [JsonPropertyName("mandatory")]
    public bool? IsMandatory { get; set; }
    [JsonConverter(typeof(SunSpecIsStaticConverter))]
    [JsonPropertyName("static")]
    public bool? IsStatic { get; set; }
    public List<Symbol> Symbols { get; set; } = new List<Symbol>();
    public List<string> Standards { get; set; } = [];

    public abstract class SunSpecBooleanConverter : JsonConverter<bool>
    {
        protected abstract string TrueValue { get; }
        protected abstract string FalseValue { get; }

        public sealed override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetString() == TrueValue;
        }

        public sealed override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value ? TrueValue : FalseValue);
        }
    }

    public class SunSpecIsStaticConverter : SunSpecBooleanConverter
    {
        protected override string TrueValue => "S";
        protected override string FalseValue => "D";
    }

    public class SunSpecIsReadOnlyConverter : SunSpecBooleanConverter
    {
        protected override string TrueValue => "R";
        protected override string FalseValue => "RW";
    }

    public class SunSpecMandatoryConverter : SunSpecBooleanConverter
    {
        protected override string TrueValue => "M";
        protected override string FalseValue => "O";
    }
}

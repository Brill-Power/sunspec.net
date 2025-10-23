/*
 * Copyright (c) 2024 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SunSpec.Models;

public class Group : EntityBase
{
    public required string Name { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required GroupType Type { get; set; }
    public required List<Point> Points { get; set; } = new List<Point>();
    public object? Count { get; set; }
    public List<Group> Groups { get; set; } = new List<Group>();
}

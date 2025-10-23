/*
 * Copyright (c) 2024 Brill Power.
 *
 * SPDX-License-Identifier: Apache-2.0
 */
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text.Json;

namespace SunSpec.Models;

public class Model : EntityBase
{
    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    [Range(1, UInt16.MaxValue)]
    public required ushort ID { get; set; }
    public required Group Group { get; set; }

    public static Model GetModel(ushort modelId)
    {
        Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"SunSpec.Models.models.json.model_{modelId}.json.gz");
        if (stream is null)
        {
            throw new KeyNotFoundException($"Model {modelId} was not found.");
        }
        using (GZipStream decompressedStream = new GZipStream(stream, CompressionMode.Decompress))
        {
            Model? model = GetModel(decompressedStream);
            if (model is null)
            {
                throw new InvalidDataException($"Unable to deserialise schema for model {modelId}.");
            }
            return model;
        }
    }

    internal static Model? GetModel(Stream stream)
    {
        return JsonSerializer.Deserialize<Model>(stream, Options);
    }
}
using Core.Abstractions;
using Core.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Core.Export;

public sealed class JsonCatalogExporter : ICatalogExporter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public void Export(IEnumerable<Product> products, string path)
    {
        ArgumentNullException.ThrowIfNull(products);

        string? directory = Path.GetDirectoryName(path);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var dtos = products
            .Select(product => product.ToDto())
            .ToList();

        string json = JsonSerializer.Serialize(dtos, Options);

        File.WriteAllText(path, json);
    }
}
using Core.Abstractions;
using Core.Export;
using System;

namespace Core.Storage;

public static class CatalogExporterFactory
{
    public static ICatalogExporter Create(string format)
    {
        return format.Trim().ToLowerInvariant() switch
        {
            "json" => new JsonCatalogExporter(),
            "csv" => new CsvCatalogExporter(),

            _ => throw new ArgumentException(
                $"Непідтримуваний формат експорту: {format}. " +
                "Доступні формати: json, csv.")
        };
    }
}
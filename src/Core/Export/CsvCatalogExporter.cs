using Core.Abstractions;
using Core.Domain;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Core.Export;

public sealed class CsvCatalogExporter : ICatalogExporter
{
    public void Export(IEnumerable<Product> products, string path)
    {
        ArgumentNullException.ThrowIfNull(products);

        string? directory = Path.GetDirectoryName(path);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var builder = new StringBuilder();

        builder.AppendLine("Id,Sku,Name,Unit,Quantity,Note");

        foreach (Product product in products)
        {
            var dto = product.ToDto();

            builder.Append(Csv(dto.Id)).Append(',');
            builder.Append(Csv(dto.Sku)).Append(',');
            builder.Append(Csv(dto.Name)).Append(',');
            builder.Append(Csv(dto.Unit)).Append(',');
            builder.Append(dto.Quantity.ToString(
                CultureInfo.InvariantCulture)).Append(',');
            builder.Append(Csv(dto.Note)).AppendLine();
        }

        File.WriteAllText(
            path,
            builder.ToString(),
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
    }

    private static string Csv(string? value)
    {
        value ??= string.Empty;

        if (value.Contains('"') ||
            value.Contains(',') ||
            value.Contains('\n') ||
            value.Contains('\r'))
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        return value;
    }
}
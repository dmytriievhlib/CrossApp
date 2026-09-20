using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Core.Dto;

namespace Core.Import;

public static class ProductJsonImporter
{
    public static ImportResult<ProductDto> Load(string path)
    {
        var items = new List<ProductDto>();
        var errors = new List<string>();

        try
        {
            string json = File.ReadAllText(path, System.Text.Encoding.UTF8);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            List<ProductDto>? products =
                JsonSerializer.Deserialize<List<ProductDto>>(json, options);

            if (products is null)
            {
                errors.Add("JSON не містить списку товарів.");
                return new ImportResult<ProductDto>(items, errors);
            }

            items.AddRange(products);
        }
        catch (JsonException ex)
        {
            errors.Add($"Помилка JSON: {ex.Message}");
        }
        catch (IOException ex)
        {
            errors.Add($"Помилка читання файлу: {ex.Message}");
        }

        return new ImportResult<ProductDto>(items, errors);
    }
}
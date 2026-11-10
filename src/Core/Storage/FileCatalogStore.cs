using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

public sealed class FileCatalogStore : ICatalogStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly Dictionary<string, Product> _cache =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly string _path;
    private bool _loaded;

    public FileCatalogStore(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException(
                "Шлях до файлу не може бути порожнім.",
                nameof(path));

        _path = Path.GetFullPath(path);
    }

    private void EnsureLoaded()
    {
        if (_loaded)
            return;

        if (File.Exists(_path))
        {
            string json = File.ReadAllText(_path);

            var dtos =
                JsonSerializer.Deserialize<List<ProductDto>>(
                    json,
                    Options) ?? [];

            foreach (ProductDto dto in dtos)
            {
                Product product = Product.FromDto(dto);
                _cache[product.Id] = product;
            }
        }

        _loaded = true;
    }

    private void Flush()
    {
        string? directory = Path.GetDirectoryName(_path);

        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var dtos = _cache.Values
            .Select(product => product.ToDto())
            .ToList();

        string json =
            JsonSerializer.Serialize(dtos, Options);

        string tempPath = _path + ".tmp";

        File.WriteAllText(tempPath, json);

        File.Move(tempPath, _path, overwrite: true);
    }

    public IReadOnlyList<Product> List()
    {
        EnsureLoaded();

        return _cache.Values.ToList();
    }

    public Product? GetById(string id)
    {
        EnsureLoaded();

        return _cache.GetValueOrDefault(id);
    }

    public void Add(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);

        EnsureLoaded();

        if (_cache.ContainsKey(item.Id))
        {
            throw new InvalidOperationException(
                $"Запис з id={item.Id} уже існує.");
        }

        _cache.Add(item.Id, item);

        Flush();
    }

    public void Update(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);

        EnsureLoaded();

        _cache[item.Id] = item;

        Flush();
    }

    public bool Remove(string id)
    {
        EnsureLoaded();

        if (!_cache.Remove(id))
            return false;

        Flush();

        return true;
    }
}
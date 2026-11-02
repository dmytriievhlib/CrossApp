using System.Collections.Generic;
using System.IO;
using System;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

public sealed class JsonCatalogImporter : ICatalogImporter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async IAsyncEnumerable<Product> StreamAsync(
        string path,
        IProgress<int>? progress = null,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ct.ThrowIfCancellationRequested();

        await using FileStream stream = new(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: true);

        int index = 0;

        await foreach (ProductDto? dto in
            JsonSerializer.DeserializeAsyncEnumerable<ProductDto>(
                stream,
                Options,
                cancellationToken: ct))
        {
            ct.ThrowIfCancellationRequested();
            index++;

            if (dto is null)
                throw new DomainException(
                    $"Порожній запис №{index} у JSON.");

            Product product;

            try
            {
                product = Product.FromDto(dto);
            }
            catch (ArgumentException ex)
            {
                throw new DomainException(
                    $"Помилка в записі №{index}: {ex.Message}", ex);
            }

            int percentage = stream.Length == 0
                ? 100
                : Math.Clamp(
                    (int)(stream.Position * 100 / stream.Length),
                    0,
                    99);

            progress?.Report(percentage);

            yield return product;
        }

        progress?.Report(100);
    }

    public async Task<IReadOnlyList<Product>> LoadAsync(
        string path,
        CancellationToken ct = default)
    {
        var products = new List<Product>();

        await foreach (Product product in StreamAsync(
            path,
            ct: ct))
        {
            products.Add(product);
        }

        return products;
    }
}
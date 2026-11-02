using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Services;
using Core.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Core.Tests;

public class ImportTests
{
    [Fact]
    public async Task ImportAsync_AddsProductsAndSkipsDuplicates()
    {
        string path = Path.GetTempFileName();

        try
        {
            await File.WriteAllTextAsync(path, """
            [
              {
                "Id": "prod001",
                "Sku": "SKU-001",
                "Name": "Test product",
                "Unit": "pcs",
                "Quantity": 10
              }
            ]
            """);

            var store = new InMemoryCatalogStore();
            var service = new CatalogService(store);
            var importer = new JsonCatalogImporter();

            int firstImport = await service.ImportAsync(importer, path);
            int secondImport = await service.ImportAsync(importer, path);

            Assert.Equal(1, firstImport);
            Assert.Equal(0, secondImport);
            Assert.Single(service.All());
            Assert.Equal(10, service.All()[0].Quantity);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task LoadAsync_InvalidProduct_ThrowsDomainException()
    {
        string path = Path.GetTempFileName();

        try
        {
            await File.WriteAllTextAsync(path, """
            [
              {
                "Id": "",
                "Sku": "SKU-001",
                "Name": "Test product",
                "Unit": "pcs",
                "Quantity": 10
              }
            ]
            """);

            var importer = new JsonCatalogImporter();

            DomainException exception =
                await Assert.ThrowsAsync<DomainException>(
                    () => importer.LoadAsync(path));

            Assert.Contains("записі №1", exception.Message);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task LoadAsync_PreCancelledToken_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var importer = new JsonCatalogImporter();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => importer.LoadAsync(
                "неіснуючий-файл.json",
                cts.Token));
    }

    [Fact]
    public async Task StreamAsync_ReportsProgress()
    {
        string path = Path.GetTempFileName();

        try
        {
            await File.WriteAllTextAsync(path, """
            [
              {
                "Id": "prod001",
                "Sku": "SKU-001",
                "Name": "Test product",
                "Unit": "pcs",
                "Quantity": 10
              }
            ]
            """);

            var importer = new JsonCatalogImporter();
            var reportedValues = new List<int>();

            var progress = new InlineTestProgress<int>(
                value => reportedValues.Add(value));

            var products = new List<Product>();

            await foreach (Product product in importer.StreamAsync(
                path,
                progress))
            {
                products.Add(product);
            }

            Assert.Single(products);
            Assert.Contains(100, reportedValues);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task ImportAsync_Timeout_ThrowsTimeoutException()
    {
        var service = new CatalogService(
            new InMemoryCatalogStore());

        var importer = new DelayedCatalogImporter();

        await Assert.ThrowsAsync<TimeoutException>(
            () => service.ImportAsync(
                importer,
                "unused.json",
                timeout: TimeSpan.FromMilliseconds(100)));
    }

    private sealed class InlineTestProgress<T> : IProgress<T>
    {
        private readonly Action<T> _handler;

        public InlineTestProgress(Action<T> handler)
        {
            _handler = handler;
        }

        public void Report(T value)
        {
            _handler(value);
        }
    }

    private sealed class DelayedCatalogImporter : ICatalogImporter
    {
        public async IAsyncEnumerable<Product> StreamAsync(
            string path,
            IProgress<int>? progress = null,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            await Task.Delay(Timeout.Infinite, ct);
            yield break;
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
}
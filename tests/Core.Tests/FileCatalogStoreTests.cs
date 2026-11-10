using Core.Domain;
using Core.Storage;
using System;
using System.IO;
using Xunit;

namespace Core.Tests;

public sealed class FileCatalogStoreTests : IDisposable
{
    private readonly string _testDirectory;

    public FileCatalogStoreTests()
    {
        _testDirectory = Path.Combine(
            Path.GetTempPath(),
            "CrossApp.Tests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_testDirectory);
    }

    [Fact]
    public void Add_NewStore_ListReturnsSavedProduct()
    {
        // Arrange
        string filePath = Path.Combine(
            _testDirectory,
            "catalog.json");

        Product expected = Product.Create(
            "test001",
            "SKU-TEST-001",
            "Тестовий товар",
            "шт.",
            25);

        // Act
        var firstStore = new FileCatalogStore(filePath);
        firstStore.Add(expected);

        var secondStore = new FileCatalogStore(filePath);
        var products = secondStore.List();

        // Assert
        Product actual = Assert.Single(products);

        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Sku, actual.Sku);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.Unit, actual.Unit);
        Assert.Equal(expected.Quantity, actual.Quantity);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }
}
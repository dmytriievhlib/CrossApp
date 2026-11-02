using Core.Domain;
using Core.Reports;
using System.Collections.Generic;
using System;
using Xunit;

namespace Core.Tests;

public class ReportServiceTests
{
    private static List<Product> CreateProducts()
    {
        return new List<Product>
        {
            Product.Create("1", "SKU-MID", "Середній товар", "шт.", 20),
            Product.Create("2", "SKU-BIG", "Великий товар", "шт.", 50),
            Product.Create("3", "SKU-LOW", "Малий товар", "кг", 3),
            Product.Create("4", "SKU-LOW2", "Ще один малий товар", "кг", 3)
        };
    }

    [Fact]
    public void TopByQuantity_ReturnsProductsInDeterministicOrder()
    {
        var service = new ReportService();

        var result = service.TopByQuantity(CreateProducts(), 2);

        Assert.Equal(2, result.Count);
        Assert.Equal("SKU-BIG", result[0].Sku);
        Assert.Equal("SKU-MID", result[1].Sku);
    }

    [Fact]
    public void LowStock_ReturnsOnlyProductsBelowThreshold()
    {
        var service = new ReportService();

        var result = service.LowStock(CreateProducts(), 10);

        Assert.Equal(2, result.Count);
        Assert.All(result, item => Assert.True(item.Quantity < 10));
        Assert.Equal("SKU-LOW", result[0].Sku);
        Assert.Equal("SKU-LOW2", result[1].Sku);
    }

    [Fact]
    public void Totals_ReturnsCorrectSummary()
    {
        var service = new ReportService();

        var result = service.Totals(CreateProducts());

        Assert.Equal(4, result.Items);
        Assert.Equal(76, result.Total);
        Assert.Equal(19, result.Average);
        Assert.Equal(50, result.Max);
    }

    [Fact]
    public void Totals_EmptyCollection_ReturnsZeros()
    {
        var service = new ReportService();

        var result = service.Totals(Array.Empty<Product>());

        Assert.Equal(0, result.Items);
        Assert.Equal(0, result.Total);
        Assert.Equal(0, result.Average);
        Assert.Equal(0, result.Max);
    }
}
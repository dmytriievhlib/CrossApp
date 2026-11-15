
using System.Net;
using System.Net.Http.Json;
using Api.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Api.Tests;

public class ProductsApiTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ProductsApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostProduct_Returns201_AndGetByLocation_Returns200()
    {
        // Arrange
        var request = new SaveProductRequest
        {
            Sku = "TEST-001",
            Name = "Інтеграційний товар",
            Unit = "шт.",
            Quantity = 5
        };

        // Act: створення товару
        var postResponse = await _client.PostAsJsonAsync(
            "/api/products",
            request);

        // Assert: перевірка створення
        Assert.Equal(
            HttpStatusCode.Created,
            postResponse.StatusCode);

        Assert.NotNull(postResponse.Headers.Location);

        // Act: отримання товару за адресою Location
        var getResponse = await _client.GetAsync(
            postResponse.Headers.Location);

        // Assert: перевірка отримання
        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode);
    }
}

using System.Net;
using System.Net.Http.Json;
using Api.Contracts;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Api.Tests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databasePath = Path.Combine(
        Path.GetTempPath(),
        $"CrossApp_Api_Tests_{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite($"Data Source={_databasePath}"));
        });
    }
}

public class ProductsApiTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public ProductsApiTests(ApiFactory factory)
    {
        _client = factory.CreateClient();

        // Створення чистої тестової бази даних
        using var scope = factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        db.Database.EnsureCreated();
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
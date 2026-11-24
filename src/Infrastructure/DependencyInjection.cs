using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string contentRootPath)
    {
        var connectionString =
            configuration.GetConnectionString("AppDb")
            ?? throw new InvalidOperationException(
                "Connection string 'AppDb' не знайдено.");

        var databasePath = connectionString
            .Replace("Data Source=", "", StringComparison.OrdinalIgnoreCase)
            .Trim();

        if (!Path.IsPathRooted(databasePath))
        {
            databasePath = Path.Combine(
                contentRootPath,
                databasePath);
        }

        var directory = Path.GetDirectoryName(databasePath);

        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var resolvedConnectionString =
            $"Data Source={databasePath}";

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(resolvedConnectionString));

        services.AddScoped<Core.Abstractions.ICatalogStore, EfCatalogStore>();

        return services;
    }
}
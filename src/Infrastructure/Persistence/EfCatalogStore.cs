using Core;
using Core.Abstractions;
using Core.Domain;
using Infrastructure.Persistence.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System;

namespace Infrastructure.Persistence;

public sealed class EfCatalogStore : ICatalogStore
{
    private readonly AppDbContext _db;

    public EfCatalogStore(AppDbContext db)
    {
        _db = db;
    }

    public IReadOnlyList<Product> List()
    {
        return _db.Products
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .Select(p => Product.Create(
                p.Id,
                p.Sku,
                p.Name,
                p.Unit,
                p.Quantity))
            .ToList();
    }

    public Product? GetById(string id)
    {
        var row = _db.Products
            .AsNoTracking()
            .FirstOrDefault(p => p.Id == id);

        return row is null
            ? null
            : Product.Create(
                row.Id,
                row.Sku,
                row.Name,
                row.Unit,
                row.Quantity);
    }

    public void Add(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);

        _db.Products.Add(new ProductRow
        {
            Id = item.Id,
            Sku = item.Sku,
            Name = item.Name,
            Unit = item.Unit,
            Quantity = item.Quantity
        });

        SaveChangesWithSkuConflictHandling();
    }

    public void Update(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var row = _db.Products
            .FirstOrDefault(p => p.Id == item.Id);

        if (row is null)
            throw new InvalidOperationException(
                $"Запис з id={item.Id} не знайдено.");

        row.Sku = item.Sku;
        row.Name = item.Name;
        row.Unit = item.Unit;
        row.Quantity = item.Quantity;

        _db.SaveChanges();
    }

    public bool Remove(string id)
    {
        var row = _db.Products
            .FirstOrDefault(p => p.Id == id);

        if (row is null)
            return false;

        _db.Products.Remove(row);
        _db.SaveChanges();

        return true;
    }
    private void SaveChangesWithSkuConflictHandling()
    {
        try
        {
            _db.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is SqliteException sqliteException &&
                sqliteException.SqliteExtendedErrorCode == 2067 &&
                sqliteException.Message.Contains(
                    "Products.Sku",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new DomainException(
                    "Товар із таким SKU уже існує.",
                    ex);
            }

            throw;
        }
    }
}
using Core.Abstractions;
using Core.Domain;
using System.Collections.Generic;
using System;

namespace Core.Storage;

public sealed class CachingCatalogStore : ICatalogStore
{
    private readonly ICatalogStore _inner;
    private readonly Dictionary<string, Product?> _cache =
        new(StringComparer.OrdinalIgnoreCase);

    public CachingCatalogStore(ICatalogStore inner)
    {
        _inner = inner
            ?? throw new ArgumentNullException(nameof(inner));
    }

    public IReadOnlyList<Product> List() =>
        _inner.List();

    public Product? GetById(string id)
    {
        if (_cache.TryGetValue(id, out Product? cached))
        {
            return cached;
        }

        Product? product = _inner.GetById(id);

        _cache[id] = product;

        return product;
    }

    public void Add(Product item)
    {
        _inner.Add(item);
        _cache.Remove(item.Id);
    }

    public void Update(Product item)
    {
        _inner.Update(item);
        _cache.Remove(item.Id);
    }

    public bool Remove(string id)
    {
        bool removed = _inner.Remove(id);
        _cache.Remove(id);

        return removed;
    }
}
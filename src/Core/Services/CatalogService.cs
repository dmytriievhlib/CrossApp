using Core.Abstractions;
using Core.Domain;
using System.Collections.Generic;
using System;

namespace Core.Services;

public sealed class CatalogService
{
    private readonly ICatalogStore _store;

    public CatalogService(ICatalogStore store)
    {
        _store = store
            ?? throw new ArgumentNullException(nameof(store));
    }

    public Product Add(
        string sku,
        string name,
        string unit,
        int quantity)
    {
        Product product = Product.Create(
            Guid.NewGuid().ToString("N")[..8],
            sku,
            name,
            unit,
            quantity);

        _store.Add(product);

        return product;
    }

    public void Receive(
        string id,
        int quantity)
    {
        Product product = _store.GetById(id)
            ?? throw new InvalidOperationException(
                $"Немає запису з id={id}.");

        product.RegisterArrival(quantity);

        _store.Update(product);
    }

    public void Issue(
        string id,
        int quantity)
    {
        Product product = _store.GetById(id)
            ?? throw new InvalidOperationException(
                $"Немає запису з id={id}.");

        product.Issue(quantity);

        _store.Update(product);
    }

    public IReadOnlyList<Product> All() =>
        _store.List();

    public Product? Find(string id) =>
        _store.GetById(id);

    public bool Remove(string id) =>
        _store.Remove(id);
}

using Core.Abstractions;
using Core.Domain;
using Core.Events;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Core.Services;

public sealed class CatalogService
{
    private readonly ICatalogStore _store;
    private readonly IReadOnlyList<INotificationSink> _sinks;

    public event EventHandler<CatalogChangedEventArgs>? Changed;

    public CatalogService(
        ICatalogStore store,
        IEnumerable<INotificationSink>? sinks = null)
    {
        _store = store
            ?? throw new ArgumentNullException(nameof(store));

        _sinks = sinks is null
    ? Array.Empty<INotificationSink>()
    : sinks.ToList();
    }

    private void OnChanged(
        Product product,
        CatalogChangeKind kind,
        int delta)
    {
        var eventArgs = new CatalogChangedEventArgs(
            product.Id,
            kind,
            delta,
            product.Quantity);

        // Механізм подій.
        Changed?.Invoke(this, eventArgs);

        // Механізм сповіщень через інтерфейс.
        string message =
            $"{eventArgs.At:HH:mm:ss} {eventArgs.Kind,-8} " +
            $"{eventArgs.Id} {eventArgs.Delta:+0;-0} " +
            $"→ залишок {eventArgs.QuantityAfter}";

        foreach (INotificationSink sink in _sinks)
        {
            sink.Notify(message);
        }
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
        OnChanged(product, CatalogChangeKind.Added, quantity);

        return product;
    }

    public void Receive(string id, int quantity)
    {
        Product product = _store.GetById(id)
            ?? throw new InvalidOperationException(
                $"Немає запису з id={id}.");

        product.RegisterArrival(quantity);
        _store.Update(product);

        OnChanged(product, CatalogChangeKind.Received, quantity);
    }

    public void Issue(string id, int quantity)
    {
        Product product = _store.GetById(id)
            ?? throw new InvalidOperationException(
                $"Немає запису з id={id}.");

        product.Issue(quantity);
        _store.Update(product);

        OnChanged(product, CatalogChangeKind.Issued, -quantity);
    }

    public IReadOnlyList<Product> All() =>
        _store.List();

    public Product? Find(string id) =>
        _store.GetById(id);

    public bool Remove(string id)
    {
        Product? product = _store.GetById(id);

        if (product is null || !_store.Remove(id))
            return false;

        OnChanged(
            product,
            CatalogChangeKind.Removed,
            -product.Quantity);

        return true;
    }

    public IReadOnlyList<Product> FindAll(
        Func<Product, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return _store.List()
            .Where(predicate)
            .ToList();
    }

    public IReadOnlyList<Product> Search(
        Func<Product, bool> predicate) =>
        FindAll(predicate);
}

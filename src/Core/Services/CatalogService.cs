
using Core.Abstractions;
using Core.Domain;
using Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

    private void OnChanged(Product product, CatalogChangeKind kind, int delta)
    {
        var eventArgs = new CatalogChangedEventArgs(
            product.Id, kind, delta, product.Quantity);

        Changed?.Invoke(this, eventArgs);

        string message =
            $"{eventArgs.At:HH:mm:ss} {eventArgs.Kind,-8} " +
            $"{eventArgs.Id} {eventArgs.Delta:+0;-0} " +
            $"→ залишок {eventArgs.QuantityAfter}";

        foreach (INotificationSink sink in _sinks)
            sink.Notify(message);
    }

    public Product Add(string sku, string name, string unit, int quantity)
    {
        Product product = Product.Create(
            Guid.NewGuid().ToString("N")[..8],
            sku, name, unit, quantity);

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

    public IReadOnlyList<Product> All() => _store.List();

    public Product? Find(string id) => _store.GetById(id);

    public bool Remove(string id)
    {
        Product? product = _store.GetById(id);

        if (product is null || !_store.Remove(id))
            return false;

        OnChanged(product, CatalogChangeKind.Removed, -product.Quantity);

        return true;
    }

    public IReadOnlyList<Product> FindAll(Func<Product, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return _store.List().Where(predicate).ToList();
    }

    public IReadOnlyList<Product> Search(Func<Product, bool> predicate) =>
        FindAll(predicate);

    public async Task<int> ImportAsync(
ICatalogImporter importer,
string path,
CancellationToken ct = default,
IProgress<int>? progress = null,
TimeSpan? timeout = null)
    {
        ArgumentNullException.ThrowIfNull(importer);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        using var timeoutCts = timeout.HasValue
            ? new CancellationTokenSource(timeout.Value)
            : null;

        using var linkedCts = timeoutCts is null
            ? CancellationTokenSource.CreateLinkedTokenSource(ct)
            : CancellationTokenSource.CreateLinkedTokenSource(
                ct,
                timeoutCts.Token);

        int added = 0;

        try
        {
            await foreach (Product item in importer.StreamAsync(
                path,
                progress,
                linkedCts.Token))
            {
                linkedCts.Token.ThrowIfCancellationRequested();

                if (_store.GetById(item.Id) is not null)
                    continue;

                _store.Add(item);
                OnChanged(
                    item,
                    CatalogChangeKind.Added,
                    item.Quantity);

                added++;
            }

            return added;
        }
        catch (OperationCanceledException)
            when (timeoutCts?.IsCancellationRequested == true
                  && !ct.IsCancellationRequested)
        {
            throw new TimeoutException(
                "Час очікування імпорту перевищено.");
        }

    }
}

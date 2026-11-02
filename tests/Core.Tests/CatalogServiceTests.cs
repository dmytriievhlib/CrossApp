using Core.Domain;
using Core.Events;
using Core.Services;
using Core.Storage;
using System;
using Xunit;

namespace Core.Tests;

public class CatalogServiceTests
{
    [Fact]
    public void Receive_WithNegativeQuantity_ThrowsAndDoesNotChangeStock()
    {
        var store = new InMemoryCatalogStore();
        var service = new CatalogService(store);

        Product product = service.Add(
            "SKU-001", "Тестовий товар", "шт.", 10);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => service.Receive(product.Id, -2));

        Assert.Equal(10, service.Find(product.Id)!.Quantity);
    }

    [Fact]
    public void Add_RaisesChangedEventExactlyOnce()
    {
        var store = new InMemoryCatalogStore();
        var service = new CatalogService(store);

        int eventCount = 0;
        CatalogChangedEventArgs? receivedEvent = null;

        service.Changed += (_, args) =>
        {
            eventCount++;
            receivedEvent = args;
        };

        Product product = service.Add(
            "SKU-002", "Новий товар", "шт.", 5);

        Assert.Equal(1, eventCount);
        Assert.NotNull(receivedEvent);
        Assert.Equal(product.Id, receivedEvent.Id);
        Assert.Equal(CatalogChangeKind.Added, receivedEvent.Kind);
        Assert.Equal(5, receivedEvent.QuantityAfter);
    }
}
using System;

namespace Core.Domain;

public sealed class Order
{
    public string Id { get; }
    public OrderStatus Status { get; private set; }

    private Order(
        string id)
    {
        Id = id;
        Status = OrderStatus.Draft;
    }

    public static Order Create(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException(
                "Ідентифікатор замовлення обов'язковий",
                nameof(id));

        return new Order(id.Trim());
    }

    public void ChangeStatus(OrderStatus newStatus)
    {
        bool allowed = (Status, newStatus) switch
        {
            (OrderStatus.Draft, OrderStatus.Confirmed) => true,
            (OrderStatus.Draft, OrderStatus.Cancelled) => true,
            _ => false
        };

        if (!allowed)
        {
            throw new InvalidOperationException(
                $"Недозволений перехід " +
                $"{Status} → {newStatus}");
        }

        Status = newStatus;
    }

    public override string ToString() =>
        $"Замовлення {Id}: {Status}";
}
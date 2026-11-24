using System;

namespace Infrastructure.Persistence.Models;

public sealed class BatchRow
{
    public int Id { get; set; }

    public string ProductId { get; set; } = null!;

    public int Amount { get; set; }

    public DateTime ReceivedAt { get; set; }

    public ProductRow Product { get; set; } = null!;
}
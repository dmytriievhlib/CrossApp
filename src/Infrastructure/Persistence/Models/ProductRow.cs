using System.Collections.Generic;

namespace Infrastructure.Persistence.Models;

public sealed class ProductRow
{
    public string Id { get; set; } = null!;

    public string Sku { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Unit { get; set; } = null!;

    public int Quantity { get; set; }

    public List<BatchRow> Batches { get; set; } = new();
}
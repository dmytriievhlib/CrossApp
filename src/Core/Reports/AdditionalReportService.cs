
using System;
using System.Collections.Generic;
using System.Linq;
using Core.Domain;

namespace Core.Reports;

public sealed class AdditionalReportService
{
    public IReadOnlyList<ProductMovementSummary> GroupJoinMovements(
        IEnumerable<Product> products,
        IEnumerable<StockMovement> movements)
    {
        ArgumentNullException.ThrowIfNull(products);
        ArgumentNullException.ThrowIfNull(movements);

        return products
            .GroupJoin(
                movements,
                product => product.Sku,
                movement => movement.Sku,
                (product, productMovements) =>
                    new ProductMovementSummary(
                        product.Sku,
                        product.Name,
                        product.Quantity,
                        productMovements.Count(),
                        productMovements.Sum(m => m.Delta)))
            .OrderByDescending(item => item.MovementCount)
            .ThenBy(item => item.Sku, StringComparer.Ordinal)
            .ToList();
    }
}

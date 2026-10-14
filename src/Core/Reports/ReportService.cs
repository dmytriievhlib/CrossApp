
using System;
using System.Collections.Generic;
using System.Linq;
using Core.Domain;

namespace Core.Reports;

public sealed class ReportService
{
    public IReadOnlyList<StockLine> LowStock(
        IEnumerable<Product> source,
        int threshold)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (threshold < 0)
            throw new ArgumentOutOfRangeException(nameof(threshold));

        return source
            .Where(p => p.Quantity < threshold)
            .OrderBy(p => p.Quantity)
            .ThenBy(p => p.Sku, StringComparer.Ordinal)
            .Select(p => new StockLine(
                p.Sku, p.Name, p.Quantity, p.Unit))
            .ToList();
    }

    public IReadOnlyList<StockLine> TopByQuantity(
        IEnumerable<Product> source,
        int n)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source
            .TopBy(p => p.Quantity, n, p => p.Sku)
            .Select(p => new StockLine(
                p.Sku, p.Name, p.Quantity, p.Unit))
            .ToList();
    }

    public IReadOnlyList<GroupSummary> ByUnit(
        IEnumerable<Product> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source
            .GroupBy(p => p.Unit, StringComparer.OrdinalIgnoreCase)
            .Select(g => new GroupSummary(
                g.Key, g.Count(), g.Sum(p => p.Quantity)))
            .OrderByDescending(g => g.TotalQuantity)
            .ThenBy(g => g.Key, StringComparer.Ordinal)
            .ToList();
    }

    public IReadOnlyList<GroupSummary> TopUnits(
        IEnumerable<Product> source,
        int n)
    {
        return ByUnit(source)
            .TopBy(g => g.TotalQuantity, n, g => g.Key)
            .ToList();
    }

    public IReadOnlyList<GroupSummary> ByUnitQuerySyntax(
        IEnumerable<Product> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var query =
            from product in source
            group product by product.Unit into unitGroup
            let count = unitGroup.Count()
            let total = unitGroup.Sum(p => p.Quantity)
            orderby total descending, unitGroup.Key
            select new GroupSummary(
                unitGroup.Key, count, total);

        return query.ToList();
    }

    public (int Items, int Total, double Average, int Max) Totals(
        IEnumerable<Product> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var products = source.ToList();

        if (products.Count == 0)
            return (0, 0, 0d, 0);

        return (
            products.Count,
            products.Sum(p => p.Quantity),
            products.Average(p => p.Quantity),
            products.Max(p => p.Quantity));
    }
}

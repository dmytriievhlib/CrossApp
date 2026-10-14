
namespace Core.Reports;

public sealed record StockMovement(
    string Sku,
    string Operation,
    int Delta);

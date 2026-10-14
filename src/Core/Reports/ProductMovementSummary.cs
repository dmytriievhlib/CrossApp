
namespace Core.Reports;

public sealed record ProductMovementSummary(
    string Sku,
    string Name,
    int CurrentQuantity,
    int MovementCount,
    int NetMovement);


namespace EnterpriseOrderManagementSystem.API.Contracts.Inventory;

public sealed record StockMovementRequest(
    int Quantity,
    string Reference,
    string Reason);

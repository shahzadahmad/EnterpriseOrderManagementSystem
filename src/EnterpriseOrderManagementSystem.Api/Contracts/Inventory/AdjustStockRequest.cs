
namespace EnterpriseOrderManagementSystem.API.Contracts.Inventory;

public sealed record AdjustStockRequest(
    int NewAvailableQuantity,
    string Reference,
    string Reason);

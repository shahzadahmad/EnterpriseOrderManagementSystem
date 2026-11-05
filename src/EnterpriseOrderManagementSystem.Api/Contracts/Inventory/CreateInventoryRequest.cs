
namespace EnterpriseOrderManagementSystem.API.Contracts.Inventory;

public sealed record CreateInventoryRequest(
    Guid ProductId,
    Guid WarehouseId,
    int ReorderLevel,
    int MaximumQuantity);

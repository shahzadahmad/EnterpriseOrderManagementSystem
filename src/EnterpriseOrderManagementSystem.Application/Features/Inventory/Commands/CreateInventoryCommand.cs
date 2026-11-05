using EnterpriseOrderManagementSystem.Application.Common.Interfaces;

namespace EnterpriseOrderManagementSystem.Application.Features.Inventory.Commands;

/// <summary>
/// Creates an inventory record for a product in a warehouse.
/// </summary>
public sealed record CreateInventoryCommand(
    Guid ProductId,
    Guid WarehouseId,
    int ReorderLevel,
    int MaximumQuantity) : ICommand<Guid>;
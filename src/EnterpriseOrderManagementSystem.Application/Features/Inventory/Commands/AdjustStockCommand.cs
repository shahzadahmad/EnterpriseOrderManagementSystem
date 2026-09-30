using EnterpriseOrderManagementSystem.Application.Common.Interfaces;

namespace EnterpriseOrderManagementSystem.Application.Features.Inventory.Commands;

public sealed record AdjustStockCommand(
    Guid InventoryId,
    int NewAvailableQuantity,
    string Reference,
    string Reason) : ICommand<Guid>;
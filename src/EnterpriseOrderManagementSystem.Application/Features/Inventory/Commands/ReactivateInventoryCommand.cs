using EnterpriseOrderManagementSystem.Application.Common.Interfaces;

namespace EnterpriseOrderManagementSystem.Application.Features.Inventory.Commands;

public sealed record ReactivateInventoryCommand(
    Guid InventoryId) : ICommand<Guid>;
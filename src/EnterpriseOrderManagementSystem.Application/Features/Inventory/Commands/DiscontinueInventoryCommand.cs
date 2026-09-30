using EnterpriseOrderManagementSystem.Application.Common.Interfaces;

namespace EnterpriseOrderManagementSystem.Application.Features.Inventory.Commands;

public sealed record DiscontinueInventoryCommand(
    Guid InventoryId,
    string Reason) : ICommand<Guid>;
using EnterpriseOrderManagementSystem.Application.Common.Interfaces;

namespace EnterpriseOrderManagementSystem.Application.Features.Inventory.Commands;

public sealed record CommitReservationCommand(
    Guid InventoryId,
    int Quantity,
    string Reference,
    string Reason) : ICommand<Guid>;

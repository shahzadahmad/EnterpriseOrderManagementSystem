using EnterpriseOrderManagementSystem.Application.Common.Interfaces;

namespace EnterpriseOrderManagementSystem.Application.Features.Orders.Commands;

public sealed record ShipOrderCommand(
    Guid OrderId) : ICommand<Guid>;
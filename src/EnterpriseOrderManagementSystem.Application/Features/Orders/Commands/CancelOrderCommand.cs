using EnterpriseOrderManagementSystem.Application.Common.Interfaces;

namespace EnterpriseOrderManagementSystem.Application.Features.Orders.Commands;

public sealed record CancelOrderCommand(
    Guid OrderId,
    string Reason) : ICommand<Guid>;
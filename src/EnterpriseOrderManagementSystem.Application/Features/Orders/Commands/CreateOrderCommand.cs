using EnterpriseOrderManagementSystem.Application.Common.Interfaces;

namespace EnterpriseOrderManagementSystem.Application.Features.Orders.Commands;

public sealed record CreateOrderCommand(
    Guid CustomerId) : ICommand<Guid>;
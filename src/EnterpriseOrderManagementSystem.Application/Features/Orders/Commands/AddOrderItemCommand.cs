using EnterpriseOrderManagementSystem.Application.Common.Interfaces;

namespace EnterpriseOrderManagementSystem.Application.Features.Orders.Commands;

public sealed record AddOrderItemCommand(
    Guid OrderId,
    Guid ProductId,
    int Quantity) : ICommand<Guid>;

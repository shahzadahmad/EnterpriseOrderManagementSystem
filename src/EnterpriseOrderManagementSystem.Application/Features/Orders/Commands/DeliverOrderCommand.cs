using EnterpriseOrderManagementSystem.Application.Common.Interfaces;

namespace EnterpriseOrderManagementSystem.Application.Features.Orders.Commands;

public sealed record DeliverOrderCommand(
    Guid OrderId) : ICommand<Guid>;
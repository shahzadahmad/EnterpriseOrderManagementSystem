using EnterpriseOrderManagementSystem.Application.Common.Interfaces;

namespace EnterpriseOrderManagementSystem.Application.Features.Orders.Commands;

public sealed record ConfirmOrderCommand(
    Guid OrderId) : ICommand<Guid>;

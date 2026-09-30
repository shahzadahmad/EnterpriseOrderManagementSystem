using EnterpriseOrderManagementSystem.Application.Common.Interfaces;

namespace EnterpriseOrderManagementSystem.Application.Features.Customers.Commands;

public sealed record ActivateCustomerCommand(
    Guid CustomerId) : ICommand<Guid>;
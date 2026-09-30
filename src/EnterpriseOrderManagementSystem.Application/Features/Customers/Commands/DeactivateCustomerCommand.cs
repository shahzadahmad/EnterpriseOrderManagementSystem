using EnterpriseOrderManagementSystem.Application.Common.Interfaces;

namespace EnterpriseOrderManagementSystem.Application.Features.Customers.Commands;

public sealed record DeactivateCustomerCommand(
    Guid CustomerId) : ICommand<Guid>;
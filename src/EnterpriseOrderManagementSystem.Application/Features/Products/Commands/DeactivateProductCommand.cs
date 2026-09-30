using EnterpriseOrderManagementSystem.Application.Common.Interfaces;

namespace EnterpriseOrderManagementSystem.Application.Features.Products.Commands;

public sealed record DeactivateProductCommand(
    Guid ProductId) : ICommand<Guid>;
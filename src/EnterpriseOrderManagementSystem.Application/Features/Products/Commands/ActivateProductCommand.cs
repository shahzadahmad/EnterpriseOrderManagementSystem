using EnterpriseOrderManagementSystem.Application.Common.Interfaces;

namespace EnterpriseOrderManagementSystem.Application.Features.Products.Commands;

public sealed record ActivateProductCommand(
    Guid ProductId) : ICommand<Guid>;
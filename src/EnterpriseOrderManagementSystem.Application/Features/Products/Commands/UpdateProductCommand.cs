using EnterpriseOrderManagementSystem.Application.Common.Interfaces;

namespace EnterpriseOrderManagementSystem.Application.Features.Products.Commands;

public sealed record UpdateProductCommand(
    Guid ProductId,
    string Name,
    string Description) : ICommand<Guid>;

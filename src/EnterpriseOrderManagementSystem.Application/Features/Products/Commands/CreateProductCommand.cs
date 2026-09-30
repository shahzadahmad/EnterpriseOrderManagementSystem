using EnterpriseOrderManagementSystem.Application.Common.Interfaces;

namespace EnterpriseOrderManagementSystem.Application.Features.Products.Commands;

public sealed record CreateProductCommand(
    string SKU,
    string Name,
    string Description,
    decimal Price,
    string Currency) : ICommand<Guid>;
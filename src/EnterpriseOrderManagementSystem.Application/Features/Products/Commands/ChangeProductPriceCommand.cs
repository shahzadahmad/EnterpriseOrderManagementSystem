using EnterpriseOrderManagementSystem.Application.Common.Interfaces;

namespace EnterpriseOrderManagementSystem.Application.Features.Products.Commands;

public sealed record ChangeProductPriceCommand(
    Guid ProductId,
    decimal Price,
    string Currency) : ICommand<Guid>;

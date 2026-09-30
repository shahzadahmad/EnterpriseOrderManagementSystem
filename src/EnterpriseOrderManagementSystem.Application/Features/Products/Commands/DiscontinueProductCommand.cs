using EnterpriseOrderManagementSystem.Application.Common.Interfaces;

namespace EnterpriseOrderManagementSystem.Application.Features.Products.Commands;

public sealed record DiscontinueProductCommand(
    Guid ProductId) : ICommand<Guid>;
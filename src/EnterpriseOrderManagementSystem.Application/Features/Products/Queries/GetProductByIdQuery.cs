using EnterpriseOrderManagementSystem.Application.DTOs.Products;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Products.Queries;

public sealed record GetProductByIdQuery(
    Guid ProductId) : IRequest<ProductDto?>;

using EnterpriseOrderManagementSystem.Application.Common.Interfaces;
using EnterpriseOrderManagementSystem.Application.DTOs.Products;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Products.Queries;

public sealed record GetProductByIdQuery(
    Guid ProductId) : IQuery<ProductDto?>;

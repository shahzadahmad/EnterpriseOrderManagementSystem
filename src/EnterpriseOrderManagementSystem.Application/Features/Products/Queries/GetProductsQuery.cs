using EnterpriseOrderManagementSystem.Application.Common.Models;
using EnterpriseOrderManagementSystem.Application.DTOs.Products;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Products.Queries;

public sealed record GetProductsQuery(
    PaginationRequest Pagination)
    : IRequest<PaginatedResult<ProductDto>>;

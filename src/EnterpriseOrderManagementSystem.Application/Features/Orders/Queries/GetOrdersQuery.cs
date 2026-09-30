using EnterpriseOrderManagementSystem.Application.Common.Models;
using EnterpriseOrderManagementSystem.Application.DTOs.Orders;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Orders.Queries;

public sealed record GetOrdersQuery(
    PaginationRequest Pagination)
    : IRequest<PaginatedResult<OrderDto>>;

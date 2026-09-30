using EnterpriseOrderManagementSystem.Application.Common.Models;
using EnterpriseOrderManagementSystem.Application.DTOs.Customers;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Customers.Queries;

public sealed record GetCustomersQuery(
    PaginationRequest Pagination)
    : IRequest<PaginatedResult<CustomerDto>>;

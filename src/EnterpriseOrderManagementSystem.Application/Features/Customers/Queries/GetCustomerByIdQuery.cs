using EnterpriseOrderManagementSystem.Application.DTOs.Customers;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Customers.Queries;

public sealed record GetCustomerByIdQuery(
    Guid CustomerId) : IRequest<CustomerDto?>;
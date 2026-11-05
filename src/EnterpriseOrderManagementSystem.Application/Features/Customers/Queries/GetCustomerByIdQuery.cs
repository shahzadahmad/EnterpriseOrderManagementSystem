using EnterpriseOrderManagementSystem.Application.Common.Interfaces;
using EnterpriseOrderManagementSystem.Application.DTOs.Customers;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Customers.Queries;

public sealed record GetCustomerByIdQuery(Guid CustomerId) : IQuery<CustomerDto?>;
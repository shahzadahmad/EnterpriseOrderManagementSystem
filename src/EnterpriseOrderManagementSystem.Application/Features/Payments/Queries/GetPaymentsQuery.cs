using EnterpriseOrderManagementSystem.Application.Common.Interfaces;
using EnterpriseOrderManagementSystem.Application.Common.Models;
using EnterpriseOrderManagementSystem.Application.DTOs.Payments;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Payments.Queries;

public sealed record GetPaymentsQuery(PaginationRequest Pagination)
    : IQuery<PaginatedResult<PaymentDto>>;
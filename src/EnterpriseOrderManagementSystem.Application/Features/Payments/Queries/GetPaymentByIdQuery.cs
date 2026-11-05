using EnterpriseOrderManagementSystem.Application.Common.Interfaces;
using EnterpriseOrderManagementSystem.Application.DTOs.Payments;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Payments.Queries;

public sealed record GetPaymentByIdQuery(Guid PaymentId) : IQuery<PaymentDto?>;
using EnterpriseOrderManagementSystem.Application.Common.Interfaces;
using EnterpriseOrderManagementSystem.Domain.Enums;

namespace EnterpriseOrderManagementSystem.Application.Features.Payments.Commands;

public sealed record CreatePaymentCommand(
    Guid OrderId,
    decimal Amount,
    string Currency,
    PaymentMethod PaymentMethod,
    PaymentProvider PaymentProvider) : ICommand<Guid>;

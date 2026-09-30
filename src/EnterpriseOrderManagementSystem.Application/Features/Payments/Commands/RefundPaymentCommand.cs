using EnterpriseOrderManagementSystem.Application.Common.Interfaces;

namespace EnterpriseOrderManagementSystem.Application.Features.Payments.Commands;

public sealed record RefundPaymentCommand(
    Guid PaymentId,
    decimal RefundAmount,
    string Reason) : ICommand<Guid>;
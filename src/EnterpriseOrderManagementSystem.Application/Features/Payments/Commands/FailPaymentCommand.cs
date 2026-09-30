using EnterpriseOrderManagementSystem.Application.Common.Interfaces;

namespace EnterpriseOrderManagementSystem.Application.Features.Payments.Commands;

public sealed record FailPaymentCommand(
    Guid PaymentId,
    string FailureCode,
    string FailureReason) : ICommand<Guid>;
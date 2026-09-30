using EnterpriseOrderManagementSystem.Domain.Enums;

namespace EnterpriseOrderManagementSystem.Application.DTOs.Payments;

public sealed record PaymentDto(
    Guid Id,
    Guid OrderId,
    decimal Amount,
    string Currency,
    PaymentMethod PaymentMethod,
    PaymentProvider PaymentProvider,
    PaymentStatus Status,
    string? ProviderReference,
    string? AuthorizationCode,
    string? FailureCode,
    string? FailureReason,
    string? CancellationReason,
    decimal TotalRefundedAmount,
    DateTime CreatedOnUtc,
    DateTime? AuthorizedOnUtc,
    DateTime? CapturedOnUtc,
    DateTime? SettledOnUtc,
    DateTime? RefundedOnUtc,
    DateTime? CancelledOnUtc,
    DateTime? FailedOnUtc,
    decimal RemainingRefundableAmount,
    bool IsRefundable,
    bool IsFullyRefunded,
    bool IsTerminal,
    IReadOnlyCollection<PaymentTransactionDto> Transactions);

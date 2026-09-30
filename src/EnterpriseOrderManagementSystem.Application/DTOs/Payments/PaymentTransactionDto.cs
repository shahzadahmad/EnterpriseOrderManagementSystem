using EnterpriseOrderManagementSystem.Domain.Enums;

namespace EnterpriseOrderManagementSystem.Application.DTOs.Payments;

public sealed record PaymentTransactionDto(
    Guid Id,
    PaymentTransactionType TransactionType,
    decimal Amount,
    string Currency,
    string ProviderReference,
    string Reason,
    DateTime OccurredOnUtc);
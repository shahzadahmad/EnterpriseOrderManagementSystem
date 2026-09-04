using EnterpriseOrderManagementSystem.Domain.Common.Events;

namespace EnterpriseOrderManagementSystem.Domain.Aggregates.PaymentAggregate.Events;

/// <summary>
/// Raised when a payment or part of a payment is refunded.
/// </summary>
public sealed record PaymentRefundedDomainEvent(
    Guid PaymentId,
    Guid OrderId,
    decimal RefundAmount,
    decimal TotalRefundedAmount,
    string RefundReference,
    string Reason) : DomainEvent;
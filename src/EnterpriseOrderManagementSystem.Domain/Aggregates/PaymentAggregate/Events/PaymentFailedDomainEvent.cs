using EnterpriseOrderManagementSystem.Domain.Common.Events;

namespace EnterpriseOrderManagementSystem.Domain.Aggregates.PaymentAggregate.Events;

/// <summary>
/// Raised when payment processing fails.
/// </summary>
public sealed record PaymentFailedDomainEvent(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount,
    string FailureCode,
    string FailureReason) : DomainEvent;
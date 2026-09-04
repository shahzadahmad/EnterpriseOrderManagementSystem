using EnterpriseOrderManagementSystem.Domain.Common.Events;

namespace EnterpriseOrderManagementSystem.Domain.Aggregates.PaymentAggregate.Events;

/// <summary>
/// Raised when a pending or authorized payment is cancelled.
/// </summary>
public sealed record PaymentCancelledDomainEvent(
    Guid PaymentId,
    Guid OrderId,
    string Reason) : DomainEvent;
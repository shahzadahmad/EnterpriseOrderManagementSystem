using EnterpriseOrderManagementSystem.Domain.Common.Events;

namespace EnterpriseOrderManagementSystem.Domain.Aggregates.PaymentAggregate.Events;

/// <summary>
/// Raised when a new payment is created.
/// </summary>
public sealed record PaymentCreatedDomainEvent(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount,
    string Currency) : DomainEvent;
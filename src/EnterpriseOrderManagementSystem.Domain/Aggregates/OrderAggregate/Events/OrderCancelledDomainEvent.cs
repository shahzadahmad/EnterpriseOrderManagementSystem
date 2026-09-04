using EnterpriseOrderManagementSystem.Domain.Common.Events;

namespace EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate.Events;

/// <summary>
/// Raised when an order is cancelled.
/// </summary>
public sealed record OrderCancelledDomainEvent(
    Guid OrderId,
    string Reason)
    : DomainEvent;
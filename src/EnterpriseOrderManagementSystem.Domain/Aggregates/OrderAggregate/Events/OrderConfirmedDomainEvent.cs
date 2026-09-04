using EnterpriseOrderManagementSystem.Domain.Common.Events;

namespace EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate.Events;

/// <summary>
/// Raised when an order is confirmed.
/// </summary>
public sealed record OrderConfirmedDomainEvent(
    Guid OrderId)
    : DomainEvent;
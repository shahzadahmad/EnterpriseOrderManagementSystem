using EnterpriseOrderManagementSystem.Domain.Common.Events;

namespace EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate.Events;

/// <summary>
/// Raised when an order has been shipped.
/// </summary>
public sealed record OrderShippedDomainEvent(
    Guid OrderId)
    : DomainEvent;
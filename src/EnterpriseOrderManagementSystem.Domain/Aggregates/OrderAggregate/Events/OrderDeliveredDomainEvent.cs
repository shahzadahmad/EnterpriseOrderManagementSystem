using EnterpriseOrderManagementSystem.Domain.Common.Events;

namespace EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate.Events;

/// <summary>
/// Raised when an order has been delivered.
/// </summary>
public sealed record OrderDeliveredDomainEvent(
    Guid OrderId)
    : DomainEvent;
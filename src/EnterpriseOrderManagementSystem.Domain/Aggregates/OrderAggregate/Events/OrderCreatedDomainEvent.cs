using EnterpriseOrderManagementSystem.Domain.Common.Events;

namespace EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate.Events;

/// <summary>
/// Raised after a new order has been created.
/// </summary>
public sealed record OrderCreatedDomainEvent(
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount)
    : DomainEvent;
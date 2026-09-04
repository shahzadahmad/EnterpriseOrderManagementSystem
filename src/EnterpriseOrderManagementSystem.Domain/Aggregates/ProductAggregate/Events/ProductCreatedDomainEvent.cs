using EnterpriseOrderManagementSystem.Domain.Common.Events;

namespace EnterpriseOrderManagementSystem.Domain.Aggregates.ProductAggregate.Events;

public sealed record ProductCreatedDomainEvent(
    Guid ProductId)
    : DomainEvent;
using EnterpriseOrderManagementSystem.Domain.Common.Events;

namespace EnterpriseOrderManagementSystem.Domain.Aggregates.ProductAggregate.Events;

public sealed record ProductDeactivatedDomainEvent(
    Guid ProductId)
    : DomainEvent;
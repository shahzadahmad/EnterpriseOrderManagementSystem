using EnterpriseOrderManagementSystem.Domain.Common.Events;

namespace EnterpriseOrderManagementSystem.Domain.Aggregates.CustomerAggregate.Events;

public sealed record CustomerDeactivatedDomainEvent(
    Guid CustomerId)
    : DomainEvent;
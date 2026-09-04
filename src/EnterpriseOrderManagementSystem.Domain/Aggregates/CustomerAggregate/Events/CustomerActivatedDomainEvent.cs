using EnterpriseOrderManagementSystem.Domain.Common.Events;

namespace EnterpriseOrderManagementSystem.Domain.Aggregates.CustomerAggregate.Events;

public sealed record CustomerActivatedDomainEvent(
    Guid CustomerId)
    : DomainEvent;
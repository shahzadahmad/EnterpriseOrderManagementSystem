using EnterpriseOrderManagementSystem.Domain.Common.Events;
using EnterpriseOrderManagementSystem.Domain.Common.ValueObjects;

namespace EnterpriseOrderManagementSystem.Domain.Aggregates.ProductAggregate.Events;

public sealed record ProductPriceChangedDomainEvent(
    Guid ProductId,
    Money OldPrice,
    Money NewPrice)
    : DomainEvent;
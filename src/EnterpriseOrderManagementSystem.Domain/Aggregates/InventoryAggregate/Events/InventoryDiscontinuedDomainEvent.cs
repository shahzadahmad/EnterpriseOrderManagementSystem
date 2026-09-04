using EnterpriseOrderManagementSystem.Domain.Common.Base;
using EnterpriseOrderManagementSystem.Domain.Common.Events;

namespace EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate.Events;

/// <summary>
/// Raised when inventory is discontinued.
/// </summary>
public sealed record InventoryDiscontinuedDomainEvent(
    Guid InventoryId,
    Guid ProductId,
    Guid WarehouseId,
    string Reason)
    : DomainEvent;
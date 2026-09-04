using EnterpriseOrderManagementSystem.Domain.Common.Base;
using EnterpriseOrderManagementSystem.Domain.Common.Events;

namespace EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate.Events;

/// <summary>
/// Raised when a new inventory record is created.
/// </summary>
public sealed record InventoryCreatedDomainEvent(
    Guid InventoryId,
    Guid ProductId,
    Guid WarehouseId)
    : DomainEvent;
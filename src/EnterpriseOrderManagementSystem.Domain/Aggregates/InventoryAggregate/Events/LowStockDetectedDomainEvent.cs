using EnterpriseOrderManagementSystem.Domain.Common.Base;
using EnterpriseOrderManagementSystem.Domain.Common.Events;

namespace EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate.Events;

/// <summary>
/// Raised when available inventory reaches
/// or falls below the reorder level.
/// </summary>
public sealed record LowStockDetectedDomainEvent(
    Guid InventoryId,
    Guid ProductId,
    Guid WarehouseId,
    int AvailableQuantity,
    int ReorderLevel)
    : DomainEvent;
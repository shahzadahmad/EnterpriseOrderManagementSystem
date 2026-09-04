using EnterpriseOrderManagementSystem.Domain.Common.Base;
using EnterpriseOrderManagementSystem.Domain.Common.Events;

namespace EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate.Events;

/// <summary>
/// Raised when inventory is manually adjusted.
/// </summary>
public sealed record StockAdjustedDomainEvent(
    Guid InventoryId,
    Guid ProductId,
    Guid WarehouseId,
    int PreviousAvailableQuantity,
    int CurrentAvailableQuantity,
    string Reference,
    string Reason)
    : DomainEvent;
using EnterpriseOrderManagementSystem.Domain.Common.Base;
using EnterpriseOrderManagementSystem.Domain.Common.Events;

namespace EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate.Events;

/// <summary>
/// Raised when reserved stock is released.
/// </summary>
public sealed record StockReleasedDomainEvent(
    Guid InventoryId,
    Guid ProductId,
    Guid WarehouseId,
    int Quantity,
    string Reference,
    string Reason)
    : DomainEvent;
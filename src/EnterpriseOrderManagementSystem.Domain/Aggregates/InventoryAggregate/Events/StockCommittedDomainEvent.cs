using EnterpriseOrderManagementSystem.Domain.Common.Base;
using EnterpriseOrderManagementSystem.Domain.Common.Events;

namespace EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate.Events;

/// <summary>
/// Raised when reserved stock is committed,
/// typically after shipment.
/// </summary>
public sealed record StockCommittedDomainEvent(
    Guid InventoryId,
    Guid ProductId,
    Guid WarehouseId,
    int Quantity,
    string Reference,
    string Reason)
    : DomainEvent;
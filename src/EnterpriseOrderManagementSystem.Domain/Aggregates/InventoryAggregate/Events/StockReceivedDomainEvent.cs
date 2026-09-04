using EnterpriseOrderManagementSystem.Domain.Common.Base;
using EnterpriseOrderManagementSystem.Domain.Common.Events;

namespace EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate.Events;

/// <summary>
/// Raised when stock is received into inventory.
/// </summary>
public sealed record StockReceivedDomainEvent(
    Guid InventoryId,
    Guid ProductId,
    Guid WarehouseId,
    int Quantity,
    string Reference,
    string Reason)
    : DomainEvent;
using EnterpriseOrderManagementSystem.Domain.Common.Base;
using EnterpriseOrderManagementSystem.Domain.Common.Events;

namespace EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate.Events;

/// <summary>
/// Raised when inventory becomes out of stock.
/// </summary>
public sealed record OutOfStockDomainEvent(
    Guid InventoryId,
    Guid ProductId,
    Guid WarehouseId)
    : DomainEvent;
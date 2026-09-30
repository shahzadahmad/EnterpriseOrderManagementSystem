using EnterpriseOrderManagementSystem.Application.DTOs.Inventory;
using EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate;

namespace EnterpriseOrderManagementSystem.Application.Mapping;

public static class InventoryMapping
{
    public static InventoryDto ToDto(this Inventory inventory)
    {
        ArgumentNullException.ThrowIfNull(inventory);

        var transactions = inventory.Transactions
            .Select(transaction => new InventoryTransactionDto(
                transaction.Id,
                transaction.MovementType,
                transaction.Quantity,
                transaction.AvailableQuantityBefore,
                transaction.AvailableQuantityAfter,
                transaction.ReservedQuantityBefore,
                transaction.ReservedQuantityAfter,
                transaction.Reference,
                transaction.Reason,
                transaction.OccurredOnUtc))
            .ToList();

        return new InventoryDto(
            inventory.Id,
            inventory.ProductId,
            inventory.WarehouseId,
            inventory.AvailableQuantity,
            inventory.ReservedQuantity,
            inventory.ReorderLevel,
            inventory.MaximumQuantity,
            inventory.IsDiscontinued,
            inventory.LastStockMovementUtc,
            inventory.TotalQuantity,
            inventory.RemainingCapacity,
            inventory.HasRemainingCapacity,
            inventory.NeedsReorder,
            inventory.IsOutOfStock,
            inventory.Status,
            transactions);
    }
}
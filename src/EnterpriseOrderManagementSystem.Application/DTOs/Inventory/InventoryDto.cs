using EnterpriseOrderManagementSystem.Domain.Enums;

namespace EnterpriseOrderManagementSystem.Application.DTOs.Inventory;

public sealed record InventoryDto(
    Guid Id,
    Guid ProductId,
    Guid WarehouseId,
    int AvailableQuantity,
    int ReservedQuantity,
    int ReorderLevel,
    int MaximumQuantity,
    bool IsDiscontinued,
    DateTime LastStockMovementUtc,
    int TotalQuantity,
    int RemainingCapacity,
    bool HasRemainingCapacity,
    bool NeedsReorder,
    bool IsOutOfStock,
    InventoryStatus Status,
    IReadOnlyCollection<InventoryTransactionDto> Transactions);

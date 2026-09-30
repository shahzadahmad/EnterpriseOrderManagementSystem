using EnterpriseOrderManagementSystem.Domain.Enums;

namespace EnterpriseOrderManagementSystem.Application.DTOs.Inventory;

public sealed record InventoryTransactionDto(
    Guid Id,
    InventoryMovementType MovementType,
    int Quantity,
    int AvailableQuantityBefore,
    int AvailableQuantityAfter,
    int ReservedQuantityBefore,
    int ReservedQuantityAfter,
    string Reference,
    string Reason,
    DateTime OccurredOnUtc);
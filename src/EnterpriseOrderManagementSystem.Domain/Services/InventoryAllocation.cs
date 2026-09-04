namespace EnterpriseOrderManagementSystem.Domain.Services;

/// <summary>
/// Represents the inventory quantity that needs to be
/// allocated for a particular product.
///
/// This is a domain result object. It does not modify
/// the Inventory aggregate.
/// </summary>
public sealed record InventoryAllocation(
    Guid ProductId,
    Guid InventoryId,
    int Quantity);
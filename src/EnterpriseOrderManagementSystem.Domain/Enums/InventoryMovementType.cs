namespace EnterpriseOrderManagementSystem.Domain.Enums;

/// <summary>
/// Represents the type of inventory movement performed
/// against an Inventory aggregate.
/// </summary>
public enum InventoryMovementType
{
    /// <summary>
    /// Stock received from a supplier,
    /// purchase order, or another warehouse.
    /// </summary>
    Receive = 1,

    /// <summary>
    /// Available stock reserved for an order.
    /// </summary>
    Reserve = 2,

    /// <summary>
    /// Previously reserved stock released
    /// back into available inventory.
    /// </summary>
    Release = 3,

    /// <summary>
    /// Reserved stock permanently committed,
    /// typically after shipment.
    /// </summary>
    Commit = 4,

    /// <summary>
    /// Manual or system-generated stock correction.
    /// </summary>
    Adjustment = 5
}
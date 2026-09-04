namespace EnterpriseOrderManagementSystem.Domain.Enums;

/// <summary>
/// Represents the calculated stock status of an inventory record.
/// This value is derived from the inventory quantities and should
/// not be persisted in the database.
/// </summary>
public enum InventoryStatus
{
    /// <summary>
    /// Stock is available.
    /// </summary>
    InStock = 1,

    /// <summary>
    /// Stock is below the configured reorder level.
    /// </summary>
    LowStock = 2,

    /// <summary>
    /// No stock is available.
    /// </summary>
    OutOfStock = 3,

    /// <summary>
    /// Product has been permanently discontinued.
    /// </summary>
    Discontinued = 4
}
using EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate;

namespace EnterpriseOrderManagementSystem.Domain.Repositories;

/// <summary>
/// Defines persistence operations for the Inventory aggregate.
///
/// Inventory represents the stock position of a specific
/// product within a specific warehouse.
/// </summary>
public interface IInventoryRepository
    : IAggregateRepository<Inventory>
{
    #region Retrieval

    /// <summary>
    /// Retrieves the inventory record for a specific
    /// product within a specific warehouse.
    ///
    /// Returns null when no matching inventory record exists.
    /// </summary>
    Task<Inventory?> GetByProductAndWarehouseAsync(
        Guid productId,
        Guid warehouseId,
        CancellationToken cancellationToken = default);

    #endregion

    #region Existence

    /// <summary>
    /// Determines whether an inventory record already exists
    /// for the specified product and warehouse combination.
    /// </summary>
    Task<bool> ExistsByProductAndWarehouseAsync(
        Guid productId,
        Guid warehouseId,
        CancellationToken cancellationToken = default);

    #endregion
}
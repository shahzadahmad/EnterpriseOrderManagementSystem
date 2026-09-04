using EnterpriseOrderManagementSystem.Domain.Aggregates.ProductAggregate;

namespace EnterpriseOrderManagementSystem.Domain.Repositories;

/// <summary>
/// Defines persistence operations for the Product aggregate.
///
/// The repository works with the Product aggregate root
/// and does not expose internal Product entities as
/// independent repositories.
/// </summary>
public interface IProductRepository
    : IAggregateRepository<Product>
{
    #region Retrieval

    /// <summary>
    /// Retrieves a product using its Stock Keeping Unit (SKU).
    ///
    /// Returns null when no matching product exists.
    /// </summary>
    Task<Product?> GetBySkuAsync(
        string sku,
        CancellationToken cancellationToken = default);

    #endregion

    #region Existence

    /// <summary>
    /// Determines whether a product with the specified
    /// SKU already exists.
    /// </summary>
    Task<bool> ExistsBySkuAsync(
        string sku,
        CancellationToken cancellationToken = default);

    #endregion
}
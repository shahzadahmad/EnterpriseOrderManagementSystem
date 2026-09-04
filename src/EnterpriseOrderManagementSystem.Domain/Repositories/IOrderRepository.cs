using EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate;

namespace EnterpriseOrderManagementSystem.Domain.Repositories;

/// <summary>
/// Defines persistence operations for the Order aggregate.
///
/// The repository works with the Order aggregate root and
/// does not expose OrderItem as an independent repository.
/// </summary>
public interface IOrderRepository
    : IAggregateRepository<Order>
{
    #region Retrieval

    /// <summary>
    /// Retrieves an order using its business order number.
    ///
    /// Returns null when no matching order exists.
    /// </summary>
    Task<Order?> GetByOrderNumberAsync(
        string orderNumber,
        CancellationToken cancellationToken = default);

    #endregion

    #region Existence

    /// <summary>
    /// Determines whether an order with the specified
    /// business order number exists.
    /// </summary>
    Task<bool> ExistsByOrderNumberAsync(
        string orderNumber,
        CancellationToken cancellationToken = default);

    #endregion
}
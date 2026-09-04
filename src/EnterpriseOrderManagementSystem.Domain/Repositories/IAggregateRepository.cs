namespace EnterpriseOrderManagementSystem.Domain.Repositories;

/// <summary>
/// Defines the common persistence operations required
/// for an Aggregate Root.
///
/// This interface contains only operations that are
/// meaningful across all Aggregate Roots.
/// </summary>
/// <typeparam name="TAggregate">
/// The Aggregate Root type.
/// </typeparam>
public interface IAggregateRepository<TAggregate>
    where TAggregate : class
{
    #region Retrieval

    /// <summary>
    /// Retrieves an aggregate by its identifier.
    ///
    /// Returns null when the aggregate does not exist.
    /// </summary>
    Task<TAggregate?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    #endregion

    #region Existence

    /// <summary>
    /// Determines whether an aggregate with the specified
    /// identifier exists.
    /// </summary>
    Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    #endregion

    #region Persistence

    /// <summary>
    /// Adds a new aggregate to the persistence boundary.
    /// </summary>
    Task AddAsync(
        TAggregate aggregate,
        CancellationToken cancellationToken = default);

    #endregion
}
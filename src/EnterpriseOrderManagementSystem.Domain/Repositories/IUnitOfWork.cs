namespace EnterpriseOrderManagementSystem.Domain.Repositories;

/// <summary>
/// Represents the Unit of Work abstraction for coordinating
/// persistence and transaction management.
///
/// The Unit of Work provides a consistent boundary for
/// persisting changes made to one or more Aggregate Roots
/// as a single atomic operation.
///
/// The Domain layer defines only the contract.
/// The actual implementation belongs to the Infrastructure layer.
///
/// Typical transaction flow:
/// BeginTransactionAsync()
///     ↓
/// SaveChangesAsync()
///     ↓
/// CommitTransactionAsync()
///
/// If an error occurs during the operation, the transaction
/// can be reverted using RollbackTransactionAsync().
/// </summary>
public interface IUnitOfWork
{
    #region Transaction Management

    /// <summary>
    /// Begins a new database transaction.
    ///
    /// All subsequent persistence operations are considered
    /// part of the same transaction until the transaction is
    /// either committed or rolled back.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation.
    /// </param>
    Task BeginTransactionAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction and permanently persists
    /// all changes that were successfully processed within it.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation.
    /// </param>
    Task CommitTransactionAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction and discards all
    /// changes made within the transaction.
    ///
    /// This is typically called when an error occurs and the
    /// operation cannot be completed successfully.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation.
    /// </param>
    Task RollbackTransactionAsync(
        CancellationToken cancellationToken = default);

    #endregion

    #region Persistence

    /// <summary>
    /// Persists all pending changes tracked by the current
    /// Unit of Work.
    ///
    /// This method writes the changes to the underlying
    /// persistence store but does not necessarily commit
    /// the surrounding transaction.
    ///
    /// When an explicit transaction is being used, the
    /// transaction should normally be committed separately
    /// using CommitTransactionAsync().
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// The number of state entries affected by the operation.
    /// </returns>
    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);

    #endregion
}

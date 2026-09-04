namespace EnterpriseOrderManagementSystem.Domain.Repositories;

/// <summary>
/// Represents the Unit of Work abstraction for the domain.
///
/// The Unit of Work coordinates persistence of changes
/// made to one or more Aggregate Roots as a single
/// persistence operation.
///
/// The Domain layer defines only the contract.
/// The actual implementation belongs to Infrastructure.
/// </summary>
public interface IUnitOfWork
{
    #region Persistence

    /// <summary>
    /// Persists all pending changes as one unit of work.
    ///
    /// Returns the number of state entries affected by
    /// the persistence operation.
    /// </summary>
    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);

    #endregion
}
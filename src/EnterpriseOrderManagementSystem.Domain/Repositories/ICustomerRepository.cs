using EnterpriseOrderManagementSystem.Domain.Aggregates.CustomerAggregate;

namespace EnterpriseOrderManagementSystem.Domain.Repositories;

/// <summary>
/// Defines persistence operations for the Customer aggregate.
///
/// The repository works with the Customer aggregate root
/// and does not expose internal Customer entities as
/// independent persistence objects.
/// </summary>
public interface ICustomerRepository
    : IAggregateRepository<Customer>
{
    #region Retrieval

    /// <summary>
    /// Retrieves a customer using the customer's email address.
    ///
    /// Returns null when no matching customer exists.
    /// </summary>
    Task<Customer?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    #endregion

    #region Existence

    /// <summary>
    /// Determines whether a customer with the specified
    /// email address already exists.
    /// </summary>
    Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    #endregion
}
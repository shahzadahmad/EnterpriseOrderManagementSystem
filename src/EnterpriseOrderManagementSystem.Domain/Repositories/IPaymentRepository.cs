using EnterpriseOrderManagementSystem.Domain.Aggregates.PaymentAggregate;

namespace EnterpriseOrderManagementSystem.Domain.Repositories;

/// <summary>
/// Defines persistence operations for the Payment aggregate.
///
/// The repository works with the Payment aggregate root
/// and does not contain payment-processing logic.
/// </summary>
public interface IPaymentRepository
    : IAggregateRepository<Payment>
{
    #region Retrieval

    /// <summary>
    /// Retrieves the payment associated with the specified order.
    ///
    /// Returns null when no payment exists for the order.
    /// </summary>
    Task<Payment?> GetByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);

    #endregion

    #region Existence

    /// <summary>
    /// Determines whether a payment already exists
    /// for the specified order.
    /// </summary>
    Task<bool> ExistsByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);

    #endregion
}
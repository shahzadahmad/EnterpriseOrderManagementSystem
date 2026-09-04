using EnterpriseOrderManagementSystem.Domain.Aggregates.PaymentAggregate;
using EnterpriseOrderManagementSystem.Domain.Enums;

namespace EnterpriseOrderManagementSystem.Domain.Specifications.PaymentSpecifications;

/// <summary>
/// Determines whether a payment is currently eligible
/// for a refund based on its payment state.
/// </summary>
public sealed class PaymentRefundableSpecification
    : Specification<Payment>
{
    #region Evaluation

    /// <summary>
    /// Determines whether the payment is in a refundable state.
    /// </summary>
    public override bool IsSatisfiedBy(Payment payment)
    {
        ArgumentNullException.ThrowIfNull(payment);

        return payment.Status == PaymentStatus.Captured
            || payment.Status == PaymentStatus.Settled;
    }

    #endregion
}
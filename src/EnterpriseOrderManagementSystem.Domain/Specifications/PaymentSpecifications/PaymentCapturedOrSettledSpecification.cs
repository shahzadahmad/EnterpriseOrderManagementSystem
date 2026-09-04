using EnterpriseOrderManagementSystem.Domain.Aggregates.PaymentAggregate;
using EnterpriseOrderManagementSystem.Domain.Enums;

namespace EnterpriseOrderManagementSystem.Domain.Specifications.PaymentSpecifications;

/// <summary>
/// Determines whether a payment has been captured
/// or settled successfully.
/// </summary>
public sealed class PaymentCapturedOrSettledSpecification
    : Specification<Payment>
{
    #region Evaluation

    /// <summary>
    /// Determines whether the payment is captured or settled.
    /// </summary>
    public override bool IsSatisfiedBy(Payment payment)
    {
        ArgumentNullException.ThrowIfNull(payment);

        return payment.Status == PaymentStatus.Captured
            || payment.Status == PaymentStatus.Settled;
    }

    #endregion
}
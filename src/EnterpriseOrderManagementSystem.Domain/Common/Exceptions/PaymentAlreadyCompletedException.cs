namespace EnterpriseOrderManagementSystem.Domain.Common.Exceptions;

/// <summary>
/// Thrown when attempting to process
/// an already completed payment.
/// </summary>
public sealed class PaymentAlreadyCompletedException
    : DomainException
{
    public PaymentAlreadyCompletedException(Guid paymentId)
        : base(
            $"Payment '{paymentId}' has already been completed.")
    {
    }
}
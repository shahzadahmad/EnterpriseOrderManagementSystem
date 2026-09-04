namespace EnterpriseOrderManagementSystem.Domain.Enums;

/// <summary>
/// Represents the type of financial transaction
/// performed against a payment.
/// </summary>
public enum PaymentTransactionType
{
    Authorization = 1,

    Capture = 2,

    Settlement = 3,

    Refund = 4
}
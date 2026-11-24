namespace EnterpriseOrderManagementSystem.Domain.Enums;

/// <summary>
/// Represents the current state of a payment.
/// </summary>
public enum PaymentStatus
{
    Pending = 1,
    Authorized = 2,
    Captured = 3,
    Failed = 4,
    Cancelled = 5,
    Refunded = 6,
    Settled = 7,
    PartiallyRefunded = 8
}
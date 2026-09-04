namespace EnterpriseOrderManagementSystem.Domain.Enums;

/// <summary>
/// Represents the current state of a payment.
/// </summary>
public enum PaymentStatus
{
    Pending = 1,

    Authorized = 2,

    Captured = 3,

    Settled = 4,

    Refunded = 5,

    Cancelled = 6,

    Failed = 7
}
namespace EnterpriseOrderManagementSystem.Domain.Enums;

/// <summary>
/// Represents the payment gateway/provider.
/// </summary>
public enum PaymentProvider
{
    None = 0,

    Stripe = 1,

    PayPal = 2,

    JazzCash = 3,

    EasyPaisa = 4,

    Bank = 5,

    Manual = 6
}
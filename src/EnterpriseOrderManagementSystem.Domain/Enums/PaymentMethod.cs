namespace EnterpriseOrderManagementSystem.Domain.Enums;

/// <summary>
/// Represents the payment method used to pay an order.
/// </summary>
public enum PaymentMethod
{
    CreditCard = 1,

    DebitCard = 2,

    BankTransfer = 3,

    CashOnDelivery = 4,

    JazzCash = 5,

    EasyPaisa = 6,

    PayPal = 7,

    Stripe = 8
}
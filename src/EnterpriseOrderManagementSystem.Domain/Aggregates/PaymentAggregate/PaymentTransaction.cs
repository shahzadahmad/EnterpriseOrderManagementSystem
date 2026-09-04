using EnterpriseOrderManagementSystem.Domain.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Enums;

namespace EnterpriseOrderManagementSystem.Domain.Aggregates.PaymentAggregate;

/// <summary>
/// Represents a single financial transaction belonging to a Payment aggregate.
///
/// PaymentTransaction is an entity owned by the Payment aggregate root.
///
/// Examples of payment transactions include:
///
/// - Authorization
/// - Capture
/// - Settlement
/// - Refund
///
/// The transaction stores the financial movement and the external
/// provider reference required for reconciliation and audit purposes.
///
/// Transactions should only be created by the Payment aggregate.
/// </summary>
public sealed class PaymentTransaction
{
    #region Constructors

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    private PaymentTransaction()
    {
    }

    /// <summary>
    /// Initializes a new payment transaction.
    /// </summary>
    private PaymentTransaction(
        Guid id,
        PaymentTransactionType transactionType,
        decimal amount,
        string currency,
        string providerReference,
        string reason,
        DateTime occurredOnUtc)
    {
        Id = id;

        TransactionType = transactionType;

        Amount = amount;

        Currency = currency;

        ProviderReference = providerReference;

        Reason = reason;

        OccurredOnUtc = occurredOnUtc;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the unique identifier of this transaction.
    /// </summary>
    public Guid Id { get; private set; }


    /// <summary>
    /// Gets the type of payment transaction.
    /// </summary>
    public PaymentTransactionType TransactionType { get; private set; }


    /// <summary>
    /// Gets the transaction amount.
    ///
    /// For refunds this represents the amount refunded
    /// in that particular refund operation.
    /// </summary>
    public decimal Amount { get; private set; }


    /// <summary>
    /// Gets the three-letter ISO currency code.
    /// </summary>
    public string Currency { get; private set; } = string.Empty;


    /// <summary>
    /// Gets the external reference returned by the
    /// payment provider.
    ///
    /// Examples:
    ///
    /// - Stripe payment reference
    /// - PayPal transaction reference
    /// - Bank transaction reference
    /// - Refund reference
    /// </summary>
    public string ProviderReference { get; private set; } = string.Empty;


    /// <summary>
    /// Gets the business reason associated with
    /// this payment transaction.
    /// </summary>
    public string Reason { get; private set; } = string.Empty;


    /// <summary>
    /// Gets the UTC timestamp when the transaction occurred.
    /// </summary>
    public DateTime OccurredOnUtc { get; private set; }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Creates a new payment transaction.
    /// </summary>
    /// <param name="transactionType">
    /// Type of payment transaction.
    /// </param>
    /// <param name="amount">
    /// Amount associated with the transaction.
    /// </param>
    /// <param name="currency">
    /// Three-letter ISO currency code.
    /// </param>
    /// <param name="providerReference">
    /// External reference returned by the payment provider.
    /// </param>
    /// <param name="reason">
    /// Business reason associated with the transaction.
    /// </param>
    /// <returns>
    /// A new PaymentTransaction entity.
    /// </returns>
    public static PaymentTransaction Create(
        PaymentTransactionType transactionType,
        decimal amount,
        string currency,
        string providerReference,
        string reason)
    {
        #region Validation

        ValidateAmount(amount);

        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            providerReference);

        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        #endregion

        #region Normalize Values

        currency = currency
            .Trim()
            .ToUpperInvariant();

        providerReference = providerReference.Trim();

        reason = reason.Trim();

        #endregion

        #region Create Entity

        return new PaymentTransaction(
            id: Guid.NewGuid(),
            transactionType: transactionType,
            amount: amount,
            currency: currency,
            providerReference: providerReference,
            reason: reason,
            occurredOnUtc: DateTime.UtcNow);

        #endregion
    }

    #endregion

    #region Validation Methods

    /// <summary>
    /// Validates the transaction amount.
    /// </summary>
    private static void ValidateAmount(decimal amount)
    {
        if (amount <= 0)
        {
            throw new BusinessRuleViolationException(
                "Payment transaction amount must be greater than zero.");
        }

        if (decimal.Round(amount, 2) != amount)
        {
            throw new BusinessRuleViolationException(
                "Payment transaction amount cannot contain more than two decimal places.");
        }
    }

    #endregion
}
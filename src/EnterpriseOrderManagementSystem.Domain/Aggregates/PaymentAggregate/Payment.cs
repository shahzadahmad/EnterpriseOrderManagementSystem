using EnterpriseOrderManagementSystem.Domain.Aggregates.PaymentAggregate.Events;
using EnterpriseOrderManagementSystem.Domain.Common;
using EnterpriseOrderManagementSystem.Domain.Common.Base;
using EnterpriseOrderManagementSystem.Domain.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Enums;

namespace EnterpriseOrderManagementSystem.Domain.Aggregates.PaymentAggregate;

/// <summary>
/// Represents the Payment aggregate root.
///
/// The Payment aggregate is responsible for protecting
/// all payment-related business rules and state transitions.
///
/// Supported lifecycle:
///
/// Pending
///     ↓
/// Authorized
///     ↓
/// Captured
///     ↓
/// Settled
///
/// Alternative terminal states:
///
/// Pending / Authorized → Cancelled
/// Pending / Authorized → Failed
///
/// Captured / Settled → Refunded
///
/// Partial refunds keep the payment in Captured or Settled
/// state until the complete refundable amount has been returned.
/// </summary>
public sealed class Payment : AggregateRoot<Guid>
{
    #region Constructors

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    private Payment()
    {
    }

    /// <summary>
    /// Initializes a new Payment aggregate.
    ///
    /// The constructor is private so that payment instances
    /// can only be created through the Create factory method.
    /// </summary>
    private Payment(
        Guid paymentId,
        Guid orderId,
        decimal amount,
        string currency,
        PaymentMethod paymentMethod,
        PaymentProvider paymentProvider)
    {
        Id = paymentId;

        OrderId = orderId;

        Amount = amount;

        Currency = currency;

        PaymentMethod = paymentMethod;

        PaymentProvider = paymentProvider;

        Status = PaymentStatus.Pending;

        TotalRefundedAmount = 0m;

        CreatedOnUtc = DateTime.UtcNow;
    }

    #endregion

    #region Properties

    #region Identity

    /// <summary>
    /// Gets the identifier of the order associated with this payment.
    /// </summary>
    public Guid OrderId { get; private set; }

    #endregion

    #region Payment Information

    /// <summary>
    /// Gets the original payment amount.
    ///
    /// This value must never be modified after payment creation.
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>
    /// Gets the three-letter ISO currency code.
    ///
    /// Examples:
    /// USD
    /// EUR
    /// GBP
    /// PKR
    /// </summary>
    public string Currency { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the payment method.
    /// </summary>
    public PaymentMethod PaymentMethod { get; private set; }

    /// <summary>
    /// Gets the payment provider.
    /// </summary>
    public PaymentProvider PaymentProvider { get; private set; }

    /// <summary>
    /// Gets the current payment status.
    /// </summary>
    public PaymentStatus Status { get; private set; }

    #endregion

    #region Provider Information

    /// <summary>
    /// Gets the external payment provider reference.
    ///
    /// This is the external reference associated with
    /// the original payment transaction.
    /// 
    /// Examples:
    /// Stripe PaymentIntent ID
    /// PayPal Transaction ID
    /// Bank Transaction ID
    /// </summary>
    public string? ProviderReference { get; private set; }

    /// <summary>
    /// Gets the authorization code returned by the
    /// payment provider.
    /// </summary>
    public string? AuthorizationCode { get; private set; }

    #endregion

    #region Failure and Cancellation Information

    /// <summary>
    /// Gets the external or internal code describing
    /// why the payment failed.
    ///
    /// Examples:
    /// - CARD_DECLINED
    /// - INSUFFICIENT_FUNDS
    /// - EXPIRED_CARD
    /// - PAYMENT_PROVIDER_ERROR
    /// - FRAUD_REJECTED
    /// </summary>
    public string? FailureCode { get; private set; }

    /// <summary>
    /// Gets the reason the payment failed.
    /// </summary>
    public string? FailureReason { get; private set; }

    /// <summary>
    /// Gets the reason the payment was cancelled.
    /// </summary>
    public string? CancellationReason { get; private set; }

    #endregion

    #region Refund Information

    /// <summary>
    /// Gets the total amount refunded so far.
    ///
    /// This allows the aggregate to support partial refunds.
    /// </summary>
    public decimal TotalRefundedAmount { get; private set; }

    #endregion

    #region Lifecycle Timestamps

    /// <summary>
    /// Gets the UTC date and time when the payment was created.
    /// </summary>
    public DateTime CreatedOnUtc { get; private set; }

    /// <summary>
    /// Gets the UTC date and time when payment authorization
    /// was successfully completed.
    /// </summary>
    public DateTime? AuthorizedOnUtc { get; private set; }

    /// <summary>
    /// Gets the UTC date and time when payment was captured.
    /// </summary>
    public DateTime? CapturedOnUtc { get; private set; }

    /// <summary>
    /// Gets the UTC date and time when payment was settled.
    /// </summary>
    public DateTime? SettledOnUtc { get; private set; }

    /// <summary>
    /// Gets the UTC date and time when the latest refund occurred.
    /// </summary>
    public DateTime? RefundedOnUtc { get; private set; }

    /// <summary>
    /// Gets the UTC date and time when payment was cancelled.
    /// </summary>
    public DateTime? CancelledOnUtc { get; private set; }

    /// <summary>
    /// Gets the UTC date and time when payment failed.
    /// </summary>
    public DateTime? FailedOnUtc { get; private set; }

    #endregion

    #region Computed Properties

    /// <summary>
    /// Gets the amount that can still be refunded.
    /// </summary>
    public decimal RemainingRefundableAmount =>
        Amount - TotalRefundedAmount;

    /// <summary>
    /// Indicates whether the payment can currently be refunded.
    /// </summary>
    public bool IsRefundable =>
        (Status == PaymentStatus.Captured ||
            Status == PaymentStatus.Settled) &&
                RemainingRefundableAmount > 0m;

    /// <summary>
    /// Indicates whether the payment has been completely refunded.
    /// </summary>
    public bool IsFullyRefunded =>
        TotalRefundedAmount == Amount;

    /// <summary>
    /// Indicates whether the payment is in a terminal state.
    /// </summary>
    public bool IsTerminal =>
        Status == PaymentStatus.Settled ||
        Status == PaymentStatus.Refunded ||
        Status == PaymentStatus.Cancelled ||
        Status == PaymentStatus.Failed;

    #endregion

    #region Payment Transaction Collections

    /// <summary>
    /// Stores the financial transactions belonging to this payment.
    ///
    /// PaymentTransaction is an entity owned by the Payment aggregate.
    /// It cannot exist independently from its Payment aggregate.
    /// </summary>
    private readonly List<PaymentTransaction> _transactions = [];

    /// <summary>
    /// Gets the financial transaction history of this payment.
    ///
    /// The collection is exposed as read-only so callers cannot
    /// directly add, remove, or replace transactions.
    ///
    /// Transactions must be created through the Payment aggregate.
    /// </summary>
    public IReadOnlyCollection<PaymentTransaction> Transactions =>
        _transactions.AsReadOnly();

    #endregion

    #endregion

    #region Factory Methods

    /// <summary>
    /// Creates a new Payment aggregate.
    ///
    /// A newly created payment always starts in the
    /// Pending state.
    /// </summary>
    /// <param name="paymentId">
    /// Unique payment identifier.
    /// </param>
    /// <param name="orderId">
    /// Identifier of the associated order.
    /// </param>
    /// <param name="amount">
    /// Original payment amount.
    /// </param>
    /// <param name="currency">
    /// ISO currency code.
    /// </param>
    /// <param name="paymentMethod">
    /// Payment method selected by the customer.
    /// </param>
    /// <param name="paymentProvider">
    /// Payment provider responsible for processing.
    /// </param>
    /// <returns>
    /// A new Payment aggregate.
    /// </returns>
    public static Payment Create(
        Guid paymentId,
        Guid orderId,
        decimal amount,
        string currency,
        PaymentMethod paymentMethod,
        PaymentProvider paymentProvider)
    {
        #region Validation

        if (paymentId == Guid.Empty)
        {
            throw new BusinessRuleViolationException(
                "Payment Id cannot be empty.");
        }

        if (orderId == Guid.Empty)
        {
            throw new BusinessRuleViolationException(
                "Order Id cannot be empty.");
        }

        ValidatePaymentAmount(amount);

        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        currency = currency.Trim().ToUpperInvariant();

        ValidateCurrency(currency);

        #endregion

        #region Create Aggregate

        var payment = new Payment(
            paymentId,
            orderId,
            amount,
            currency,
            paymentMethod,
            paymentProvider);

        #endregion

        #region Domain Event

        payment.RaisePaymentCreatedEvent();

        #endregion

        return payment;
    }

    #endregion

    #region Payment Operations

    /// <summary>
    /// Authorizes the payment through the configured payment provider.
    ///
    /// Authorization confirms that the payment provider has approved
    /// the payment amount, but the funds have not yet been captured.
    ///
    /// Valid transition:
    ///
    /// Pending → Authorized
    ///
    /// The payment provider reference and authorization code are stored
    /// for reconciliation and future capture operations.
    /// </summary>
    /// <param name="providerReference">
    /// Unique reference returned by the payment provider.
    /// </param>
    /// <param name="authorizationCode">
    /// Authorization code returned by the payment provider.
    /// </param>
    public void Authorize(
        string providerReference,
        string authorizationCode)
    {
        #region Validation

        EnsureCanAuthorize();

        ValidateProviderReference(providerReference);

        ValidateAuthorizationCode(authorizationCode);

        #endregion

        #region Normalize Values

        providerReference = providerReference.Trim();

        authorizationCode = authorizationCode.Trim();

        #endregion

        #region Update Payment State

        Status = PaymentStatus.Authorized;

        ProviderReference = providerReference;

        AuthorizationCode = authorizationCode;

        AuthorizedOnUtc = DateTime.UtcNow;

        #endregion

        #region Record Transaction

        AddTransaction(
            transactionType: PaymentTransactionType.Authorization,
            amount: Amount,
            providerReference: ProviderReference,
            reason: "Payment authorization");

        #endregion

        #region Domain Event

        RaisePaymentAuthorizedEvent();

        #endregion
    }

    /// <summary>
    /// Captures an authorized payment.
    ///
    /// Capture represents the point at which the payment provider
    /// actually captures the authorized funds.
    ///
    /// Valid transition:
    ///
    /// Authorized → Captured
    ///
    /// A payment cannot be captured unless it has first
    /// been successfully authorized.
    /// </summary>
    /// <param name="providerReference">
    /// Payment provider reference associated with the capture.
    ///
    /// If the provider returns a new capture reference,
    /// that reference should be supplied here.
    /// </param>
    public void Capture(string providerReference)
    {
        #region Validation

        EnsureCanCapture();

        ValidateProviderReference(providerReference);

        #endregion

        #region Normalize Values

        providerReference = providerReference.Trim();

        #endregion

        #region Update Payment State

        Status = PaymentStatus.Captured;

        ProviderReference = providerReference;

        CapturedOnUtc = DateTime.UtcNow;

        #endregion

        #region Record Transaction

        AddTransaction(
            transactionType: PaymentTransactionType.Capture,
            amount: Amount,
            providerReference: ProviderReference,
            reason: "Payment capture");

        #endregion

        #region Domain Event

        RaisePaymentCapturedEvent();

        #endregion
    }

    /// <summary>
    /// Settles a captured payment.
    ///
    /// Settlement represents the finalization of the payment
    /// by the payment provider.
    ///
    /// Valid transition:
    ///
    /// Captured → Settled
    ///
    /// A payment cannot be settled unless it has first
    /// been successfully captured.
    /// </summary>
    /// <param name="providerReference">
    /// External payment provider reference associated
    /// with the settlement.
    /// </param>
    public void Settle(string providerReference)
    {
        #region Validation

        EnsureCanSettle();

        ValidateProviderReference(providerReference);

        #endregion

        #region Normalize Values

        providerReference = providerReference.Trim();

        #endregion

        #region Update Payment State

        Status = PaymentStatus.Settled;

        ProviderReference = providerReference;

        SettledOnUtc = DateTime.UtcNow;

        #endregion

        #region Record Transaction

        AddTransaction(
            transactionType: PaymentTransactionType.Settlement,
            amount: Amount,
            providerReference: ProviderReference,
            reason: "Payment settlement");

        #endregion

        #region Domain Event

        RaisePaymentSettledEvent();

        #endregion
    }

    /// <summary>
    /// Refunds part or all of a captured or settled payment.
    ///
    /// Partial refunds are supported.
    ///
    /// Valid transitions:
    ///
    /// Captured → Captured
    /// Settled  → Settled
    ///
    /// When the complete refundable amount has been refunded:
    ///
    /// Captured → Refunded
    /// Settled  → Refunded
    ///
    /// Every refund must have its own provider reference
    /// for financial reconciliation and audit purposes.
    /// </summary>
    /// <param name="refundAmount">
    /// Amount to refund.
    /// </param>
    /// <param name="refundReference">
    /// Unique reference returned by the payment provider
    /// for the refund transaction.
    /// </param>
    /// <param name="reason">
    /// Business reason for the refund.
    /// </param>
    public void Refund(
        decimal refundAmount,
        string refundReference,
        string reason)
    {
        #region Validation

        EnsureCanRefund();

        ValidateRefundAmount(refundAmount);

        ValidateRefundReference(refundReference);

        ValidateRefundReason(reason);

        #endregion

        #region Normalize Values

        refundReference = refundReference.Trim();

        reason = reason.Trim();

        #endregion

        #region Calculate Refund

        var newTotalRefundedAmount =
            TotalRefundedAmount + refundAmount;

        var remainingRefundableAmount =
            Amount - newTotalRefundedAmount;

        #endregion

        #region Update Payment State

        TotalRefundedAmount = newTotalRefundedAmount;

        RefundedOnUtc = DateTime.UtcNow;

        // If the entire payment amount has been refunded,
        // the payment moves to the Refunded terminal state.
        if (remainingRefundableAmount == 0)
        {
            Status = PaymentStatus.Refunded;
        }

        #endregion

        #region Record Transaction

        AddTransaction(
            transactionType: PaymentTransactionType.Refund,
            amount: refundAmount,
            providerReference: refundReference,
            reason: reason);

        #endregion

        #region Domain Event

        RaisePaymentRefundedEvent(
            refundAmount,
            refundReference,
            reason);

        #endregion
    }

    /// <summary>
    /// Cancels the payment.
    ///
    /// Cancellation is allowed only when the payment has not
    /// been captured.
    ///
    /// Valid transitions:
    ///
    /// Pending    → Cancelled
    /// Authorized → Cancelled
    ///
    /// Captured and Settled payments cannot be cancelled.
    /// They must be refunded instead.
    /// </summary>
    /// <param name="reason">
    /// Business reason for cancelling the payment.
    /// </param>
    public void Cancel(string reason)
    {
        #region Validation

        EnsureCanCancel();

        ValidateCancellationReason(reason);

        #endregion

        #region Normalize Values

        reason = reason.Trim();

        #endregion

        #region Update Payment State

        Status = PaymentStatus.Cancelled;

        CancellationReason = reason;

        CancelledOnUtc = DateTime.UtcNow;

        #endregion

        #region Domain Event

        RaisePaymentCancelledEvent(reason);

        #endregion
    }

    /// <summary>
    /// Marks the payment as failed.
    ///
    /// A payment can fail while it is still being processed.
    /// Once a payment has been captured or settled, it cannot
    /// be marked as failed because money has already been captured.
    ///
    /// Valid transitions:
    ///
    /// Pending    → Failed
    /// Authorized → Failed
    ///
    /// Captured and Settled payments cannot transition to Failed.
    /// </summary>
    /// <param name="failureCode">
    /// External or internal code describing the payment failure.
    /// </param>
    /// <param name="failureReason">
    /// Human-readable explanation of the failure.
    /// </param>
    public void Fail(
        string failureCode,
        string failureReason)
    {
        #region Validation

        EnsureCanFail();

        ValidateFailureCode(failureCode);

        ValidateFailureReason(failureReason);

        #endregion

        #region Normalize Values

        failureCode = failureCode.Trim();

        failureReason = failureReason.Trim();

        #endregion

        #region Update Payment State

        Status = PaymentStatus.Failed;

        FailureCode = failureCode;

        FailureReason = failureReason;

        FailedOnUtc = DateTime.UtcNow;

        #endregion

        #region Domain Event

        RaisePaymentFailedEvent(
            failureCode,
            failureReason);

        #endregion
    }


    #endregion

    #region Domain Event Methods

    /// <summary>
    /// Raises the PaymentCreated domain event.
    /// </summary>
    private void RaisePaymentCreatedEvent()
    {
        AddDomainEvent(
            new PaymentCreatedDomainEvent(
                Id,
                OrderId,
                Amount,
                Currency));
    }

    /// <summary>
    /// Raises the PaymentAuthorized domain event.
    /// </summary>
    private void RaisePaymentAuthorizedEvent()
    {
        AddDomainEvent(
            new PaymentAuthorizedDomainEvent(
                Id,
                OrderId,
                Amount,
                ProviderReference!));
    }

    /// <summary>
    /// Raises the PaymentCaptured domain event.
    /// </summary>
    private void RaisePaymentCapturedEvent()
    {
        AddDomainEvent(
            new PaymentCapturedDomainEvent(
                Id,
                OrderId,
                Amount,
                ProviderReference!));
    }

    /// <summary>
    /// Raises the PaymentSettled domain event.
    /// </summary>
    private void RaisePaymentSettledEvent()
    {
        AddDomainEvent(
            new PaymentSettledDomainEvent(
                Id,
                OrderId,
                Amount,
                ProviderReference!));
    }

    /// <summary>
    /// Raises the PaymentRefunded domain event.
    ///
    /// The event contains the refund-specific provider reference
    /// so downstream systems can reconcile the refund independently
    /// from the original payment.
    /// </summary>
    /// <param name="refundAmount">
    /// Amount refunded in the current operation.
    /// </param>
    /// <param name="refundReference">
    /// Provider reference for the refund transaction.
    /// </param>
    /// <param name="reason">
    /// Business reason for the refund.
    /// </param>
    private void RaisePaymentRefundedEvent(
        decimal refundAmount,
        string refundReference,
        string reason)
    {
        AddDomainEvent(
            new PaymentRefundedDomainEvent(
                Id,
                OrderId,
                refundAmount,
                TotalRefundedAmount,
                refundReference,
                reason));
    }

    /// <summary>
    /// Raises the PaymentCancelled domain event.
    /// </summary>
    /// <param name="reason">
    /// Business reason for the cancellation.
    /// </param>
    private void RaisePaymentCancelledEvent(
        string reason)
    {
        AddDomainEvent(
            new PaymentCancelledDomainEvent(
                Id,
                OrderId,
                reason));
    }

    /// <summary>
    /// Raises the PaymentFailed domain event.
    /// </summary>
    /// <param name="failureCode">
    /// Code identifying the payment failure.
    /// </param>
    /// <param name="failureReason">
    /// Explanation of the payment failure.
    /// </param>
    private void RaisePaymentFailedEvent(
        string failureCode,
        string failureReason)
    {
        AddDomainEvent(
            new PaymentFailedDomainEvent(
                Id,
                OrderId,
                Amount,
                failureCode,
                failureReason));
    }

    #endregion

    #region Validation Methods

    /// <summary>
    /// Ensures that the payment is in a state that allows authorization.
    ///
    /// Only Pending payments can be authorized.
    /// </summary>
    private void EnsureCanAuthorize()
    {
        if (Status != PaymentStatus.Pending)
        {
            throw new BusinessRuleViolationException(
                $"Payment cannot be authorized when its current status is '{Status}'.");
        }
    }

    /// <summary>
    /// Ensures that the payment is in a state that allows capture.
    ///
    /// Only an Authorized payment can be captured.
    /// </summary>
    private void EnsureCanCapture()
    {
        if (Status != PaymentStatus.Authorized)
        {
            throw new BusinessRuleViolationException(
                $"Payment cannot be captured when its current status is '{Status}'.");
        }

        if (string.IsNullOrWhiteSpace(ProviderReference))
        {
            throw new BusinessRuleViolationException(
                "Payment cannot be captured without a provider reference.");
        }

        if (string.IsNullOrWhiteSpace(AuthorizationCode))
        {
            throw new BusinessRuleViolationException(
                "Payment cannot be captured without an authorization code.");
        }
    }

    /// <summary>
    /// Ensures that the payment is in a state that allows settlement.
    ///
    /// Only a Captured payment can be settled.
    /// </summary>
    private void EnsureCanSettle()
    {
        if (Status != PaymentStatus.Captured)
        {
            throw new BusinessRuleViolationException(
                $"Payment cannot be settled when its current status is '{Status}'.");
        }

        if (string.IsNullOrWhiteSpace(ProviderReference))
        {
            throw new BusinessRuleViolationException(
                "Payment cannot be settled without a provider reference.");
        }

        if (!CapturedOnUtc.HasValue)
        {
            throw new BusinessRuleViolationException(
                "Payment cannot be settled without a capture timestamp.");
        }
    }

    /// <summary>
    /// Ensures that the payment is currently eligible for a refund.
    ///
    /// Only Captured and Settled payments can be refunded.
    /// </summary>
    private void EnsureCanRefund()
    {
        if (Status != PaymentStatus.Captured &&
            Status != PaymentStatus.Settled)
        {
            throw new BusinessRuleViolationException(                
                $"Payment cannot be refund when its current status is '{Status}'.");
        }        

        if (TotalRefundedAmount >= Amount)
        {
            throw new BusinessRuleViolationException(
                "Payment has already been fully refunded.");
        }

        if (string.IsNullOrWhiteSpace(ProviderReference))
        {
            throw new BusinessRuleViolationException(
                "Payment cannot be refunded without a provider reference.");
        }
    }

    /// <summary>
    /// Ensures that the payment is in a state that allows cancellation.
    ///
    /// Only Pending and Authorized payments can be cancelled.
    /// </summary>
    private void EnsureCanCancel()
    {
        if (Status != PaymentStatus.Pending &&
            Status != PaymentStatus.Authorized)
        {
            throw new BusinessRuleViolationException(
                $"Payment cannot be cancelled when its current status is '{Status}'.");
        }
    }

    /// <summary>
    /// Ensures that the payment is in a state that allows failure.
    ///
    /// Only Pending and Authorized payments can transition
    /// to Failed.
    /// </summary>
    private void EnsureCanFail()
    {
        if (Status != PaymentStatus.Pending &&
            Status != PaymentStatus.Authorized)
        {
            throw new BusinessRuleViolationException(
                $"Payment cannot be marked as failed when its current status is '{Status}'.");
        }
    }

    /// <summary>
    /// Validates the payment provider reference returned
    /// by the payment provider.
    /// </summary>
    /// <param name="providerReference">
    /// External payment provider reference.
    /// </param>
    private static void ValidateProviderReference(
        string providerReference)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            providerReference);

        if (providerReference.Length > 200)
        {
            throw new BusinessRuleViolationException(
                "Provider reference cannot exceed 200 characters.");
        }
    }

    /// <summary>
    /// Validates the authorization code returned
    /// by the payment provider.
    /// </summary>
    /// <param name="authorizationCode">
    /// Authorization code returned by the provider.
    /// </param>
    private static void ValidateAuthorizationCode(
        string authorizationCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            authorizationCode);

        if (authorizationCode.Length > 100)
        {
            throw new BusinessRuleViolationException(
                "Authorization code cannot exceed 100 characters.");
        }
    }

    /// <summary>
    /// Validates the requested refund amount.
    ///
    /// The refund must:
    ///
    /// - Be greater than zero.
    /// - Not exceed the remaining refundable amount.
    /// </summary>
    /// <param name="refundAmount">
    /// Requested refund amount.
    /// </param>
    private void ValidateRefundAmount(
        decimal refundAmount)
    {
        if (refundAmount <= 0)
        {
            throw new BusinessRuleViolationException(
                "Refund amount must be greater than zero.");
        }

        if (refundAmount > RemainingRefundableAmount)
        {
            throw new BusinessRuleViolationException(
                "Refund amount cannot exceed the remaining refundable amount.");
        }
    }

    /// <summary>
    /// Validates the external payment provider reference
    /// associated with the refund transaction.
    /// </summary>
    /// <param name="refundReference">
    /// Unique refund reference returned by the provider.
    /// </param>
    private static void ValidateRefundReference(
        string refundReference)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            refundReference);

        if (refundReference.Trim().Length > 200)
        {
            throw new BusinessRuleViolationException(
                "Refund reference cannot exceed 200 characters.");
        }
    }

    /// <summary>
    /// Validates the business reason supplied for the refund.
    /// </summary>
    /// <param name="reason">
    /// Business reason for the refund.
    /// </param>
    private static void ValidateRefundReason(
        string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        if (reason.Trim().Length > 500)
        {
            throw new BusinessRuleViolationException(
                "Refund reason cannot exceed 500 characters.");
        }
    }

    /// <summary>
    /// Validates the business reason supplied for cancellation.
    /// </summary>
    /// <param name="reason">
    /// Business reason for cancelling the payment.
    /// </param>
    private static void ValidateCancellationReason(
        string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        if (reason.Trim().Length > 500)
        {
            throw new BusinessRuleViolationException(
                "Cancellation reason cannot exceed 500 characters.");
        }
    }

    /// <summary>
    /// Validates the payment failure code.
    /// </summary>
    /// <param name="failureCode">
    /// Code identifying the payment failure.
    /// Examples of failure codes could be:
    /// CARD_DECLINED
    /// INSUFFICIENT_FUNDS
    /// EXPIRED_CARD
    /// PAYMENT_PROVIDER_ERROR
    /// FRAUD_REJECTED
    /// </param>
    private static void ValidateFailureCode(
        string failureCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            failureCode);

        if (failureCode.Trim().Length > 100)
        {
            throw new BusinessRuleViolationException(
                "Failure code cannot exceed 100 characters.");
        }
    }

    /// <summary>
    /// Validates the human-readable payment failure reason.
    /// </summary>
    /// <param name="failureReason">
    /// Explanation of why the payment failed.
    /// </param>
    private static void ValidateFailureReason(
        string failureReason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            failureReason);

        if (failureReason.Trim().Length > 500)
        {
            throw new BusinessRuleViolationException(
                "Failure reason cannot exceed 500 characters.");
        }
    }

    /// <summary>
    /// Validates the original payment amount.
    /// </summary>
    private static void ValidatePaymentAmount(
        decimal amount)
    {
        if (amount <= 0)
        {
            throw new BusinessRuleViolationException(
                "Payment amount must be greater than zero.");
        }

        if (decimal.Round(amount, 2) != amount)
        {
            throw new BusinessRuleViolationException(
                "Payment amount cannot contain more than two decimal places.");
        }
    }

    /// <summary>
    /// Validates the payment currency.
    /// </summary>
    private static void ValidateCurrency(
        string currency)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        currency = currency.Trim();

        if (currency.Length != 3)
        {
            throw new BusinessRuleViolationException(
                "Currency must be a valid three-letter currency code.");
        }
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Records a financial transaction belonging to this payment.
    ///
    /// This method is intentionally private because the Payment
    /// aggregate must control all transaction creation.
    /// </summary>
    private void AddTransaction(
        PaymentTransactionType transactionType,
        decimal amount,
        string providerReference,
        string reason)
    {
        var transaction = PaymentTransaction.Create(
            transactionType,
            amount,
            Currency,
            providerReference,
            reason);

        _transactions.Add(transaction);
    }

    #endregion

}
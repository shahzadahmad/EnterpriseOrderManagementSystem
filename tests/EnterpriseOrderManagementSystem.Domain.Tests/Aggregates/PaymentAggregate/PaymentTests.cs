using EnterpriseOrderManagementSystem.Domain.Aggregates.PaymentAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.PaymentAggregate.Events;
using EnterpriseOrderManagementSystem.Domain.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Enums;

namespace EnterpriseOrderManagementSystem.Domain.Tests.Aggregates.PaymentAggregate;

public sealed class PaymentTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidData_ShouldCreatePayment()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var amount = 100m;
        var currency = "USD";
        var paymentMethod = PaymentMethod.BankTransfer;
        var paymentProvider = PaymentProvider.Bank;

        // Act
        var payment = Payment.Create(
            paymentId,
            orderId,
            amount,
            currency,
            paymentMethod,
            paymentProvider);

        // Assert
        Assert.NotNull(payment);
        Assert.Equal(paymentId, payment.Id);
        Assert.Equal(orderId, payment.OrderId);
        Assert.Equal(amount, payment.Amount);
        Assert.Equal(currency, payment.Currency);
        Assert.Equal(paymentMethod, payment.PaymentMethod);
        Assert.Equal(paymentProvider, payment.PaymentProvider);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.Equal(0m, payment.TotalRefundedAmount);
        Assert.Empty(payment.Transactions);
    }

    [Fact]
    public void Create_ShouldGeneratePendingPayment()
    {
        // Arrange & Act
        var payment = CreateValidPayment();

        // Assert
        Assert.Equal(PaymentStatus.Pending, payment.Status);
    }

    [Fact]
    public void Create_ShouldInitializeEmptyTransactions()
    {
        // Arrange & Act
        var payment = CreateValidPayment();

        // Assert
        Assert.NotNull(payment.Transactions);
        Assert.Empty(payment.Transactions);
    }

    #endregion

    #region Authorize Tests

    [Fact]
    public void Authorize_FromPending_ShouldAuthorizePayment()
    {
        // Arrange
        var payment = CreateValidPayment();

        // Act
        payment.Authorize(
            providerReference: "AUTH-123",
            authorizationCode: "AUTH-CODE-123");

        // Assert
        Assert.Equal(PaymentStatus.Authorized, payment.Status);
        Assert.Equal("AUTH-123", payment.ProviderReference);
        Assert.Equal("AUTH-CODE-123", payment.AuthorizationCode);
    }

    [Fact]
    public void Authorize_ShouldCreateAuthorizationTransaction()
    {
        // Arrange
        var payment = CreateValidPayment();

        // Act
        payment.Authorize(
            "AUTH-123",
            "AUTH-CODE-123");

        // Assert
        var transaction = Assert.Single(payment.Transactions);

        Assert.Equal(
            PaymentTransactionType.Authorization,
            transaction.TransactionType);

        Assert.Equal(
            payment.Amount,
            transaction.Amount);

        Assert.Equal(
            "AUTH-123",
            transaction.ProviderReference);
    }

    [Fact]
    public void Authorize_ShouldTrimProviderReferenceAndAuthorizationCode()
    {
        // Arrange
        var payment = CreateValidPayment();

        // Act
        payment.Authorize(
            "  AUTH-123  ",
            "  AUTH-CODE-123  ");

        // Assert
        Assert.Equal("AUTH-123", payment.ProviderReference);
        Assert.Equal("AUTH-CODE-123", payment.AuthorizationCode);
    }

    [Fact]
    public void Authorize_WhenAlreadyAuthorized_ShouldThrow()
    {
        // Arrange
        var payment = CreateAuthorizedPayment();

        // Act & Assert
        Assert.Throws<BusinessRuleViolationException>(() =>
            payment.Authorize(
                "AUTH-456",
                "AUTH-CODE-456"));
    }

    #endregion

    #region Capture Tests

    [Fact]
    public void Capture_FromAuthorized_ShouldCapturePayment()
    {
        // Arrange
        var payment = CreateAuthorizedPayment();

        // Act
        payment.Capture(payment.ProviderReference!);

        // Assert
        Assert.Equal(PaymentStatus.Captured, payment.Status);
    }

    [Fact]
    public void Capture_ShouldCreateCaptureTransaction()
    {
        // Arrange
        var payment = CreateAuthorizedPayment();

        // Act
        payment.Capture(payment.ProviderReference!);

        // Assert
        Assert.Equal(2, payment.Transactions.Count);

        var transaction = payment.Transactions
            .Single(x =>
                x.TransactionType ==
                PaymentTransactionType.Capture);

        Assert.Equal(payment.Amount, transaction.Amount);
        Assert.Equal(
            payment.ProviderReference,
            transaction.ProviderReference);
    }

    [Fact]
    public void Capture_FromPending_ShouldThrow()
    {
        // Arrange
        var payment = CreateValidPayment();

        // Act & Assert
        Assert.Throws<BusinessRuleViolationException>(() =>
            payment.Capture(payment.ProviderReference!));
    }

    [Fact]
    public void Capture_WhenAlreadyCaptured_ShouldThrow()
    {
        // Arrange
        var payment = CreateAuthorizedPayment();

        payment.Capture(payment.ProviderReference!);

        // Act & Assert
        Assert.Throws<BusinessRuleViolationException>(() =>
            payment.Capture(payment.ProviderReference!));
    }

    #endregion

    #region Settle Tests

    [Fact]
    public void Settle_FromCaptured_ShouldSettlePayment()
    {
        // Arrange
        var payment = CreateCapturedPayment();

        // Act
        payment.Settle(payment.ProviderReference!);

        // Assert
        Assert.Equal(PaymentStatus.Settled, payment.Status);
    }

    [Fact]
    public void Settle_ShouldCreateSettlementTransaction()
    {
        // Arrange
        var payment = CreateCapturedPayment();

        // Act
        payment.Settle(payment.ProviderReference!);

        // Assert
        Assert.Equal(3, payment.Transactions.Count);

        var transaction = payment.Transactions
            .Single(x =>
                x.TransactionType ==
                PaymentTransactionType.Settlement);

        Assert.Equal(payment.Amount, transaction.Amount);
        Assert.Equal(
            payment.ProviderReference,
            transaction.ProviderReference);
    }

    [Fact]
    public void Settle_FromPending_ShouldThrow()
    {
        // Arrange
        var payment = CreateValidPayment();

        // Act & Assert
        Assert.Throws<BusinessRuleViolationException>(() =>
            payment.Settle(payment.ProviderReference!));
    }

    #endregion

    #region Fail Tests

    [Fact]
    public void Fail_FromPending_ShouldFailPayment()
    {
        // Arrange
        var payment = CreateValidPayment();

        // Act
        payment.Fail("CARD_DECLINED", "Card expiry date is not correct!");

        // Assert
        Assert.Equal(PaymentStatus.Failed, payment.Status);
        Assert.Equal("CARD_DECLINED", payment.FailureCode);
    }

    [Fact]
    public void Fail_ShouldTrimFailureCode()
    {
        // Arrange
        var payment = CreateValidPayment();

        // Act
        payment.Fail("  CARD_DECLINED  ", "Card expiry date is not correct!");

        // Assert
        Assert.Equal("CARD_DECLINED", payment.FailureCode);
    }

    [Fact]
    public void Fail_WithEmptyFailureCode_ShouldThrow()
    {
        // Arrange
        var payment = CreateValidPayment();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            payment.Fail(" ", "Card expiry date is not correct!"));
    }

    #endregion

    #region Cancel Tests

    [Fact]
    public void Cancel_FromPending_ShouldCancelPayment()
    {
        // Arrange
        var payment = CreateValidPayment();

        // Act
        payment.Cancel("No amount in account!");

        // Assert
        Assert.Equal(PaymentStatus.Cancelled, payment.Status);
    }

    [Fact]
    public void Cancel_FromCaptured_ShouldThrow()
    {
        // Arrange
        var payment = CreateCapturedPayment();

        // Act & Assert
        Assert.Throws<BusinessRuleViolationException>(() =>
            payment.Cancel("No amount in account!"));
    }

    #endregion

    #region Refund Tests

    [Fact]
    public void Refund_FromSettled_ShouldRefundPayment()
    {
        // Arrange
        var payment = CreateSettledPayment();

        // Act
        payment.Refund(
            refundAmount: 100m,
            refundReference: "REF-123",
            reason: "Customer requested refund");

        // Assert
        Assert.Equal(PaymentStatus.Refunded, payment.Status);
        Assert.Equal(100m, payment.TotalRefundedAmount);
        Assert.NotNull(payment.RefundedOnUtc);
    }

    [Fact]
    public void Refund_PartialAmount_ShouldKeepPaymentActive()
    {
        // Arrange
        var payment = CreateSettledPayment();

        // Act
        payment.Refund(
            refundAmount: 30m,
            refundReference: "REF-123",
            reason: "Partial refund");

        // Assert
        Assert.Equal(PaymentStatus.Settled, payment.Status);
        Assert.Equal(30m, payment.TotalRefundedAmount);
    }

    [Fact]
    public void Refund_ShouldCreateRefundTransaction()
    {
        // Arrange
        var payment = CreateSettledPayment();

        // Act
        payment.Refund(
            refundAmount: 30m,
            refundReference: "REF-123",
            reason: "Customer requested partial refund");

        // Assert
        Assert.Equal(4, payment.Transactions.Count);

        var transaction = payment.Transactions
            .Single(x =>
                x.TransactionType ==
                PaymentTransactionType.Refund);

        Assert.Equal(30m, transaction.Amount);
        Assert.Equal(
            "REF-123",
            transaction.ProviderReference);

        Assert.Equal(
            "Customer requested partial refund",
            transaction.Reason);
    }

    [Fact]
    public void Refund_MultiplePartialRefunds_ShouldTrackTotal()
    {
        // Arrange
        var payment = CreateSettledPayment();

        // Act
        payment.Refund(
            30m,
            "REF-001",
            "First refund");

        payment.Refund(
            20m,
            "REF-002",
            "Second refund");

        // Assert
        Assert.Equal(50m, payment.TotalRefundedAmount);
        Assert.Equal(PaymentStatus.Settled, payment.Status);

        Assert.Equal(
            5,
            payment.Transactions.Count);
    }

    [Fact]
    public void Refund_FullAmountAfterPartialRefund_ShouldSetRefunded()
    {
        // Arrange
        var payment = CreateSettledPayment();

        payment.Refund(
            30m,
            "REF-001",
            "First refund");

        // Act
        payment.Refund(
            70m,
            "REF-002",
            "Final refund");

        // Assert
        Assert.Equal(100m, payment.TotalRefundedAmount);
        Assert.Equal(PaymentStatus.Refunded, payment.Status);
    }

    [Fact]
    public void Refund_ExceedingRemainingAmount_ShouldThrow()
    {
        // Arrange
        var payment = CreateSettledPayment();

        payment.Refund(
            60m,
            "REF-001",
            "Partial refund");

        // Act & Assert
        Assert.Throws<BusinessRuleViolationException>(() =>
            payment.Refund(
                50m,
                "REF-002",
                "Invalid refund"));
    }

    [Fact]
    public void Refund_WithZeroAmount_ShouldThrow()
    {
        // Arrange
        var payment = CreateSettledPayment();

        // Act & Assert
        Assert.Throws<BusinessRuleViolationException>(() =>
            payment.Refund(
                0m,
                "REF-123",
                "Invalid refund"));
    }

    [Fact]
    public void Refund_WithEmptyReference_ShouldThrow()
    {
        // Arrange
        var payment = CreateSettledPayment();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            payment.Refund(
                20m,
                " ",
                "Customer refund"));
    }

    [Fact]
    public void Refund_WithEmptyReason_ShouldThrow()
    {
        // Arrange
        var payment = CreateSettledPayment();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            payment.Refund(
                20m,
                "REF-123",
                " "));
    }

    #endregion

    #region Transaction History Tests

    [Fact]
    public void CompletePaymentLifecycle_ShouldCreateCorrectTransactionHistory()
    {
        // Arrange
        var payment = CreateValidPayment();

        // Act
        payment.Authorize(
            "AUTH-123",
            "AUTH-CODE-123");

        payment.Capture(payment.ProviderReference!);

        payment.Settle(payment.ProviderReference!);

        payment.Refund(
            25m,
            "REF-123",
            "Partial refund");

        // Assert
        Assert.Equal(4, payment.Transactions.Count);

        Assert.Equal(
            PaymentTransactionType.Authorization,
            payment.Transactions.ElementAt(0).TransactionType);

        Assert.Equal(
            PaymentTransactionType.Capture,
            payment.Transactions.ElementAt(1).TransactionType);

        Assert.Equal(
            PaymentTransactionType.Settlement,
            payment.Transactions.ElementAt(2).TransactionType);

        Assert.Equal(
            PaymentTransactionType.Refund,
            payment.Transactions.ElementAt(3).TransactionType);
    }

    [Fact]
    public void Transactions_ShouldBeReadOnly()
    {
        // Arrange
        var payment = CreateAuthorizedPayment();

        // Act & Assert
        Assert.IsAssignableFrom<
            IReadOnlyCollection<PaymentTransaction>>(
            payment.Transactions);
    }

    #endregion

    #region Domain Event Tests

    [Fact]
    public void Authorize_ShouldRaiseAuthorizationEvent()
    {
        // Arrange
        var payment = CreateValidPayment();

        // Act
        payment.Authorize(
            "AUTH-123",
            "AUTH-CODE-123");

        // Assert
        Assert.NotEmpty(payment.DomainEvents);

        Assert.Contains(
            payment.DomainEvents,
            x => x.GetType() ==
                 typeof(PaymentAuthorizedDomainEvent));
    }

    [Fact]
    public void Capture_ShouldRaiseCaptureEvent()
    {
        // Arrange
        var payment = CreateAuthorizedPayment();

        // Act
        payment.Capture(payment.ProviderReference!);

        // Assert
        Assert.Contains(
            payment.DomainEvents,
            x => x.GetType() ==
                 typeof(PaymentCapturedDomainEvent));
    }

    [Fact]
    public void Settle_ShouldRaiseSettlementEvent()
    {
        // Arrange
        var payment = CreateCapturedPayment();

        // Act
        payment.Settle(payment.ProviderReference!);

        // Assert
        Assert.Contains(
            payment.DomainEvents,
            x => x.GetType() ==
                 typeof(PaymentSettledDomainEvent));
    }

    #endregion

    #region Test Helpers

    private static Payment CreateValidPayment()
    {
        return Payment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            100m,
            "USD",
            PaymentMethod.EasyPaisa,
            PaymentProvider.EasyPaisa);
    }

    private static Payment CreateAuthorizedPayment()
    {
        var payment = CreateValidPayment();
        
        payment.Authorize(
            "AUTH-123",
            "AUTH-CODE-123");

        return payment;
    }

    private static Payment CreateCapturedPayment()
    {
        var payment = CreateAuthorizedPayment();

        payment.Capture(payment.ProviderReference!);

        return payment;
    }

    private static Payment CreateSettledPayment()
    {
        var payment = CreateCapturedPayment();

        payment.Settle(payment.ProviderReference!);

        return payment;
    }

    #endregion
}

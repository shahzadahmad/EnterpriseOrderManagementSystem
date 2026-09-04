using EnterpriseOrderManagementSystem.Domain.Aggregates.PaymentAggregate;
using EnterpriseOrderManagementSystem.Domain.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Enums;

namespace EnterpriseOrderManagementSystem.Domain.Tests.Aggregates.PaymentAggregate;

public sealed class PaymentTransactionTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidData_ShouldCreateTransaction()
    {
        // Arrange
        var transactionType = PaymentTransactionType.Authorization;
        var amount = 100m;
        var currency = "USD";
        var providerReference = "AUTH-123";
        var reason = "Payment authorization";

        // Act
        var transaction = PaymentTransaction.Create(
            transactionType,
            amount,
            currency,
            providerReference,
            reason);

        // Assert
        Assert.NotNull(transaction);
        Assert.NotEqual(Guid.Empty, transaction.Id);
        Assert.Equal(transactionType, transaction.TransactionType);
        Assert.Equal(amount, transaction.Amount);
        Assert.Equal(currency, transaction.Currency);
        Assert.Equal(providerReference, transaction.ProviderReference);
        Assert.Equal(reason, transaction.Reason);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        // Act
        var transaction1 = CreateValidTransaction();
        var transaction2 = CreateValidTransaction();

        // Assert
        Assert.NotEqual(transaction1.Id, transaction2.Id);
    }

    [Fact]
    public void Create_ShouldSetOccurredOnUtc()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var transaction = CreateValidTransaction();

        var after = DateTime.UtcNow;

        // Assert
        Assert.InRange(
            transaction.OccurredOnUtc,
            before,
            after);
    }

    #endregion

    #region Amount Validation Tests

    [Fact]
    public void Create_WithZeroAmount_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<BusinessRuleViolationException>(() =>
            PaymentTransaction.Create(
                PaymentTransactionType.Authorization,
                0m,
                "USD",
                "AUTH-123",
                "Authorization"));
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<BusinessRuleViolationException>(() =>
            PaymentTransaction.Create(
                PaymentTransactionType.Authorization,
                -10m,
                "USD",
                "AUTH-123",
                "Authorization"));
    }

    [Fact]
    public void Create_WithMoreThanTwoDecimalPlaces_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<BusinessRuleViolationException>(() =>
            PaymentTransaction.Create(
                PaymentTransactionType.Authorization,
                100.123m,
                "USD",
                "AUTH-123",
                "Authorization"));
    }

    [Theory]
    [InlineData(1.00)]
    [InlineData(10.50)]
    [InlineData(99.99)]
    [InlineData(1000.25)]
    public void Create_WithValidAmount_ShouldSucceed(
        double amount)
    {
        // Arrange
        var decimalAmount = (decimal)amount;

        // Act
        var transaction = PaymentTransaction.Create(
            PaymentTransactionType.Authorization,
            decimalAmount,
            "USD",
            "AUTH-123",
            "Authorization");

        // Assert
        Assert.Equal(decimalAmount, transaction.Amount);
    }

    #endregion

    #region Currency Validation Tests

    [Fact]
    public void Create_WithEmptyCurrency_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            PaymentTransaction.Create(
                PaymentTransactionType.Authorization,
                100m,
                "",
                "AUTH-123",
                "Authorization"));
    }

    [Fact]
    public void Create_WithWhitespaceCurrency_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            PaymentTransaction.Create(
                PaymentTransactionType.Authorization,
                100m,
                "   ",
                "AUTH-123",
                "Authorization"));
    }

    [Fact]
    public void Create_ShouldNormalizeCurrencyToUpperCase()
    {
        // Act
        var transaction = PaymentTransaction.Create(
            PaymentTransactionType.Authorization,
            100m,
            " usd ",
            "AUTH-123",
            "Authorization");

        // Assert
        Assert.Equal("USD", transaction.Currency);
    }

    #endregion

    #region Provider Reference Validation Tests

    [Fact]
    public void Create_WithEmptyProviderReference_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            PaymentTransaction.Create(
                PaymentTransactionType.Authorization,
                100m,
                "USD",
                "",
                "Authorization"));
    }

    [Fact]
    public void Create_WithWhitespaceProviderReference_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            PaymentTransaction.Create(
                PaymentTransactionType.Authorization,
                100m,
                "USD",
                "   ",
                "Authorization"));
    }

    [Fact]
    public void Create_ShouldTrimProviderReference()
    {
        // Act
        var transaction = PaymentTransaction.Create(
            PaymentTransactionType.Authorization,
            100m,
            "USD",
            "  AUTH-123  ",
            "Authorization");

        // Assert
        Assert.Equal(
            "AUTH-123",
            transaction.ProviderReference);
    }

    #endregion

    #region Reason Validation Tests

    [Fact]
    public void Create_WithEmptyReason_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            PaymentTransaction.Create(
                PaymentTransactionType.Authorization,
                100m,
                "USD",
                "AUTH-123",
                ""));
    }

    [Fact]
    public void Create_WithWhitespaceReason_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            PaymentTransaction.Create(
                PaymentTransactionType.Authorization,
                100m,
                "USD",
                "AUTH-123",
                "   "));
    }

    [Fact]
    public void Create_ShouldTrimReason()
    {
        // Act
        var transaction = PaymentTransaction.Create(
            PaymentTransactionType.Authorization,
            100m,
            "USD",
            "AUTH-123",
            "  Payment authorization  ");

        // Assert
        Assert.Equal(
            "Payment authorization",
            transaction.Reason);
    }

    #endregion

    #region Transaction Type Tests

    [Theory]
    [InlineData(PaymentTransactionType.Authorization)]
    [InlineData(PaymentTransactionType.Capture)]
    [InlineData(PaymentTransactionType.Settlement)]
    [InlineData(PaymentTransactionType.Refund)]
    public void Create_WithSupportedTransactionType_ShouldSetType(
        PaymentTransactionType transactionType)
    {
        // Act
        var transaction = PaymentTransaction.Create(
            transactionType,
            100m,
            "USD",
            "PROVIDER-123",
            "Payment transaction");

        // Assert
        Assert.Equal(
            transactionType,
            transaction.TransactionType);
    }

    #endregion

    #region Immutability Tests

    [Fact]
    public void Create_ShouldExposeExpectedTransactionData()
    {
        // Arrange
        var transaction = PaymentTransaction.Create(
            PaymentTransactionType.Capture,
            100m,
            "USD",
            "CAP-123",
            "Payment capture");

        // Assert
        Assert.NotEqual(Guid.Empty, transaction.Id);
        Assert.Equal(
            PaymentTransactionType.Capture,
            transaction.TransactionType);

        Assert.Equal(100m, transaction.Amount);
        Assert.Equal("USD", transaction.Currency);
        Assert.Equal("CAP-123", transaction.ProviderReference);
        Assert.Equal("Payment capture", transaction.Reason);
    }

    #endregion

    #region Test Helpers

    private static PaymentTransaction CreateValidTransaction()
    {
        return PaymentTransaction.Create(
            PaymentTransactionType.Authorization,
            100m,
            "USD",
            "AUTH-123",
            "Payment authorization");
    }

    #endregion
}
using EnterpriseOrderManagementSystem.Domain.Aggregates.PaymentAggregate;
using EnterpriseOrderManagementSystem.Domain.Enums;
using EnterpriseOrderManagementSystem.Infrastructure.Persistence.Repositories;
using EnterpriseOrderManagementSystem.Infrastructure.Tests.Database;

namespace EnterpriseOrderManagementSystem.Infrastructure.Tests.Repositories;

[Collection("SQL Server")]
public sealed class PaymentRepositoryTests
{
    private readonly SqlServerFixture _fixture;

    public PaymentRepositoryTests(
        SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    #region Add And Get By Id With Transactions

    [Fact]
    public async Task AddAsync_ThenGetByIdAsync_ShouldReturnPaymentWithTransactions()
    {
        // Arrange
        await using var context =
            _fixture.CreateDbContext();

        var repository =
            new PaymentRepository(context);

        var paymentId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        var payment =
            Payment.Create(
                paymentId: paymentId,
                orderId: orderId,
                amount: 1000m,
                currency: "USD",
                paymentMethod: PaymentMethod.CreditCard,
                paymentProvider: PaymentProvider.Stripe);

        // Authorization transaction.
        payment.Authorize(
            providerReference: "pi-test-authorization",
            authorizationCode: "AUTH-TEST-001");

        // Capture transaction.
        payment.Capture(
            providerReference: "cap-test-001");

        // Settlement transaction.
        payment.Settle(
            providerReference: "set-test-001");

        // Partial refund transaction.
        // The payment remains Settled because only part of the
        // original amount has been refunded.
        payment.Refund(
            refundAmount: 250m,
            refundReference: "ref-test-001",
            reason: "Integration test partial refund");

        // Capture the expected persisted transaction values.
        var expectedTransactions = payment.Transactions
            .Select(transaction => new
            {
                transaction.Id,
                transaction.TransactionType,
                transaction.Amount,
                transaction.Currency,
                transaction.ProviderReference,
                transaction.Reason,
                transaction.OccurredOnUtc
            })
            .ToList();

        // Capture expected payment values before persistence.
        var expectedAmount = payment.Amount;
        var expectedCurrency = payment.Currency;
        var expectedPaymentMethod = payment.PaymentMethod;
        var expectedPaymentProvider = payment.PaymentProvider;
        var expectedStatus = payment.Status;

        var expectedProviderReference =
            payment.ProviderReference;

        var expectedAuthorizationCode =
            payment.AuthorizationCode;

        var expectedTotalRefundedAmount =
            payment.TotalRefundedAmount;

        var expectedCreatedOnUtc =
            payment.CreatedOnUtc;

        var expectedAuthorizedOnUtc =
            payment.AuthorizedOnUtc;

        var expectedCapturedOnUtc =
            payment.CapturedOnUtc;

        var expectedSettledOnUtc =
            payment.SettledOnUtc;

        var expectedRefundedOnUtc =
            payment.RefundedOnUtc;

        // Act: add and save the payment and its transactions.
        await repository.AddAsync(payment);

        await context.SaveChangesAsync();

        // Use a fresh DbContext to verify database persistence.
        await using var verificationContext =
            _fixture.CreateDbContext();

        var verificationRepository =
            new PaymentRepository(verificationContext);

        var result =
            await verificationRepository.GetByIdAsync(
                paymentId);

        // Assert: payment was persisted and retrieved.
        Assert.NotNull(result);

        Assert.Equal(paymentId, result.Id);
        Assert.Equal(orderId, result.OrderId);

        // Assert: payment information.
        Assert.Equal(expectedAmount, result.Amount);
        Assert.Equal(expectedCurrency, result.Currency);
        Assert.Equal(expectedPaymentMethod, result.PaymentMethod);
        Assert.Equal(expectedPaymentProvider, result.PaymentProvider);
        Assert.Equal(expectedStatus, result.Status);

        // Assert: provider information.
        Assert.Equal(
            expectedProviderReference,
            result.ProviderReference);

        Assert.Equal(
            expectedAuthorizationCode,
            result.AuthorizationCode);

        // Assert: refund information.
        Assert.Equal(
            expectedTotalRefundedAmount,
            result.TotalRefundedAmount);

        Assert.Equal(250m, result.TotalRefundedAmount);
        Assert.Equal(750m, result.RemainingRefundableAmount);
        Assert.True(result.IsRefundable);
        Assert.False(result.IsFullyRefunded);

        // Assert: lifecycle timestamps.
        Assert.Equal(
            expectedCreatedOnUtc,
            result.CreatedOnUtc);

        Assert.Equal(
            expectedAuthorizedOnUtc,
            result.AuthorizedOnUtc);

        Assert.Equal(
            expectedCapturedOnUtc,
            result.CapturedOnUtc);

        Assert.Equal(
            expectedSettledOnUtc,
            result.SettledOnUtc);

        Assert.Equal(
            expectedRefundedOnUtc,
            result.RefundedOnUtc);

        Assert.NotNull(result.AuthorizedOnUtc);
        Assert.NotNull(result.CapturedOnUtc);
        Assert.NotNull(result.SettledOnUtc);
        Assert.NotNull(result.RefundedOnUtc);

        // Assert: all four transactions were persisted and loaded.
        Assert.Equal(
            expectedTransactions.Count,
            result.Transactions.Count);

        Assert.Equal(4, result.Transactions.Count);

        // Assert: transaction details survived the round-trip.
        foreach (var expectedTransaction in expectedTransactions)
        {
            var actualTransaction = Assert.Single(
                result.Transactions.Where(
                    transaction =>
                        transaction.Id == expectedTransaction.Id));

            Assert.Equal(
                expectedTransaction.TransactionType,
                actualTransaction.TransactionType);

            Assert.Equal(
                expectedTransaction.Amount,
                actualTransaction.Amount);

            Assert.Equal(
                expectedTransaction.Currency,
                actualTransaction.Currency);

            Assert.Equal(
                expectedTransaction.ProviderReference,
                actualTransaction.ProviderReference);

            Assert.Equal(
                expectedTransaction.Reason,
                actualTransaction.Reason);

            Assert.Equal(
                expectedTransaction.OccurredOnUtc,
                actualTransaction.OccurredOnUtc);
        }

        // Assert: verify the expected transaction sequence and amounts.
        Assert.Collection(
            result.Transactions.OrderBy(
                transaction => transaction.OccurredOnUtc),
            transaction =>
            {
                Assert.Equal(
                    PaymentTransactionType.Authorization,
                    transaction.TransactionType);

                Assert.Equal(1000m, transaction.Amount);
                Assert.Equal(
                    "pi-test-authorization",
                    transaction.ProviderReference);
            },
            transaction =>
            {
                Assert.Equal(
                    PaymentTransactionType.Capture,
                    transaction.TransactionType);

                Assert.Equal(1000m, transaction.Amount);
                Assert.Equal(
                    "cap-test-001",
                    transaction.ProviderReference);
            },
            transaction =>
            {
                Assert.Equal(
                    PaymentTransactionType.Settlement,
                    transaction.TransactionType);

                Assert.Equal(1000m, transaction.Amount);
                Assert.Equal(
                    "set-test-001",
                    transaction.ProviderReference);
            },
            transaction =>
            {
                Assert.Equal(
                    PaymentTransactionType.Refund,
                    transaction.TransactionType);

                Assert.Equal(250m, transaction.Amount);
                Assert.Equal(
                    "ref-test-001",
                    transaction.ProviderReference);

                Assert.Equal(
                    "Integration test partial refund",
                    transaction.Reason);
            });

        // The repository eagerly loads the transaction collection.
        // Verify that the collection is available after reloading.
        Assert.Equal(4, result.Transactions.Count);
    }

    #endregion

    #region Get By Id When Payment Does Not Exist

    [Fact]
    public async Task GetByIdAsync_WhenPaymentDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        await using var context =
            _fixture.CreateDbContext();

        var repository =
            new PaymentRepository(context);

        var paymentId = Guid.NewGuid();

        // Act
        var result =
            await repository.GetByIdAsync(paymentId);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Exists

    [Fact]
    public async Task ExistsAsync_WhenPaymentExists_ShouldReturnTrue()
    {
        // Arrange
        await using var context =
            _fixture.CreateDbContext();

        var repository =
            new PaymentRepository(context);

        var payment =
            Payment.Create(
                paymentId: Guid.NewGuid(),
                orderId: Guid.NewGuid(),
                amount: 500m,
                currency: "USD",
                paymentMethod: PaymentMethod.CreditCard,
                paymentProvider: PaymentProvider.Stripe);

        await repository.AddAsync(payment);

        await context.SaveChangesAsync();

        // Act
        var exists =
            await repository.ExistsAsync(payment.Id);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_WhenPaymentDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        await using var context =
            _fixture.CreateDbContext();

        var repository =
            new PaymentRepository(context);

        // Act
        var exists =
            await repository.ExistsAsync(Guid.NewGuid());

        // Assert
        Assert.False(exists);
    }

    #endregion

    #region Get By Order Id

    [Fact]
    public async Task GetByOrderIdAsync_WhenPaymentExists_ShouldReturnPaymentWithTransactions()
    {
        // Arrange
        await using var context =
            _fixture.CreateDbContext();

        var repository =
            new PaymentRepository(context);

        var orderId = Guid.NewGuid();

        var payment =
            Payment.Create(
                paymentId: Guid.NewGuid(),
                orderId: orderId,
                amount: 750m,
                currency: "USD",
                paymentMethod: PaymentMethod.CreditCard,
                paymentProvider: PaymentProvider.Stripe);

        payment.Authorize(
            providerReference: "pi-order-test-001",
            authorizationCode: "AUTH-ORDER-001");

        await repository.AddAsync(payment);

        await context.SaveChangesAsync();

        // Verify through a fresh context.
        await using var verificationContext =
            _fixture.CreateDbContext();

        var verificationRepository =
            new PaymentRepository(verificationContext);

        // Act
        var result =
            await verificationRepository.GetByOrderIdAsync(orderId);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(orderId, result.OrderId);
        Assert.Equal(payment.Id, result.Id);

        Assert.Single(result.Transactions);

        var transaction = Assert.Single(result.Transactions);

        Assert.Equal(
            PaymentTransactionType.Authorization,
            transaction.TransactionType);

        Assert.Equal(
            "pi-order-test-001",
            transaction.ProviderReference);
    }

    #endregion

    #region Exists By Order Id

    [Fact]
    public async Task ExistsByOrderIdAsync_WhenPaymentExists_ShouldReturnTrue()
    {
        // Arrange
        await using var context =
            _fixture.CreateDbContext();

        var repository =
            new PaymentRepository(context);

        var orderId = Guid.NewGuid();

        var payment =
            Payment.Create(
                paymentId: Guid.NewGuid(),
                orderId: orderId,
                amount: 250m,
                currency: "USD",
                paymentMethod: PaymentMethod.DebitCard,
                paymentProvider: PaymentProvider.Stripe);

        await repository.AddAsync(payment);

        await context.SaveChangesAsync();

        // Act
        var exists =
            await repository.ExistsByOrderIdAsync(orderId);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsByOrderIdAsync_WhenPaymentDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        await using var context =
            _fixture.CreateDbContext();

        var repository =
            new PaymentRepository(context);

        // Act
        var exists =
            await repository.ExistsByOrderIdAsync(Guid.NewGuid());

        // Assert
        Assert.False(exists);
    }

    #endregion
}

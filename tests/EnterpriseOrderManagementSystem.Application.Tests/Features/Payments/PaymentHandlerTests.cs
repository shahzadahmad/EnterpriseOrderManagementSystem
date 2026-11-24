
using EnterpriseOrderManagementSystem.Application.Abstractions.Services.PaymentGateway;
using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Application.Common.Models;
using EnterpriseOrderManagementSystem.Application.Features.Payments.Commands;
using EnterpriseOrderManagementSystem.Application.Features.Payments.Queries;
using EnterpriseOrderManagementSystem.Application.Abstractions.Persistence;
using EnterpriseOrderManagementSystem.Application.Tests.Common;
using EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.PaymentAggregate;
using EnterpriseOrderManagementSystem.Domain.Enums;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using Moq;

namespace EnterpriseOrderManagementSystem.Application.Tests.Features.Payments;

public sealed class PaymentHandlerTests
{

    [Fact]
    public async Task CreatePayment_ShouldCreateAndPersistPayment()
    {
        // Arrange
        var order = TestData.ConfirmedOrder();

        var payments = new Mock<IPaymentRepository>();
        var orders = new Mock<IOrderRepository>();

        // Return an existing confirmed order.
        orders
            .Setup(x => x.GetByIdAsync(
                order.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // No existing payment for this order.
        payments
            .Setup(x => x.ExistsByOrderIdAsync(
                order.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        Payment? added = null;

        // Capture the payment passed to the repository.
        payments
            .Setup(x => x.AddAsync(
                It.IsAny<Payment>(),
                It.IsAny<CancellationToken>()))
            .Callback<Payment, CancellationToken>(
                (payment, _) => added = payment)
            .Returns(Task.CompletedTask);

        var handler = new CreatePaymentCommandHandler(
            payments.Object,
            orders.Object);

        var command = new CreatePaymentCommand(
            order.Id,
            order.TotalAmount.Amount,
            order.TotalAmount.Currency,
            PaymentMethod.CreditCard,
            PaymentProvider.Stripe);

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        Assert.NotNull(added);

        Assert.Equal(result, added.Id);
        Assert.Equal(order.Id, added.OrderId);
        Assert.Equal(order.TotalAmount.Amount, added.Amount);
        Assert.Equal(order.TotalAmount.Currency, added.Currency);

        // Verify repository interactions.
        orders.Verify(
            x => x.GetByIdAsync(
                order.Id,
                It.IsAny<CancellationToken>()),
            Times.Once);

        payments.Verify(
            x => x.ExistsByOrderIdAsync(
                order.Id,
                It.IsAny<CancellationToken>()),
            Times.Once);

        payments.Verify(
            x => x.AddAsync(
                It.IsAny<Payment>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreatePayment_ShouldThrowWhenOrderDoesNotExist()
    {
        var orderId = Guid.NewGuid();
        var payments = new Mock<IPaymentRepository>();
        var orders = new Mock<IOrderRepository>();

        orders.Setup(x => x.ExistsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            new CreatePaymentCommandHandler(payments.Object, orders.Object).Handle(
                new CreatePaymentCommand(orderId, 100m, "USD", PaymentMethod.CreditCard, PaymentProvider.Stripe),
                CancellationToken.None));
    }

    [Fact]
    public async Task CreatePayment_ShouldThrowConflictWhenPaymentAlreadyExists()
    {
        // Arrange
        var order = TestData.ConfirmedOrder();

        var payments = new Mock<IPaymentRepository>();
        var orders = new Mock<IOrderRepository>();

        // Return an existing confirmed order.
        orders
            .Setup(x => x.GetByIdAsync(
                order.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Simulate an existing payment.
        payments
            .Setup(x => x.ExistsByOrderIdAsync(
                order.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new CreatePaymentCommandHandler(
            payments.Object,
            orders.Object);

        var command = new CreatePaymentCommand(
            order.Id,
            order.TotalAmount.Amount,
            order.TotalAmount.Currency,
            PaymentMethod.CreditCard,
            PaymentProvider.Stripe);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(
            () => handler.Handle(
                command,
                CancellationToken.None));

        // Verify that no new payment was created.
        payments.Verify(
            x => x.AddAsync(
                It.IsAny<Payment>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task AuthorizePayment_ShouldAuthorizeWhenGatewaySucceeds()
    {
        var payment = TestData.Payment();
        var repo = PaymentRepo(payment);
        var gateway = new Mock<IPaymentGateway>();

        gateway.Setup(x => x.AuthorizeAsync(
                It.IsAny<PaymentAuthorizationRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentAuthorizationResult(
                true, "provider-ref", "auth-code", null, null));

        var result = await CreateAuthorizeHandler(repo, gateway).Handle(
            new AuthorizePaymentCommand(payment.Id),
            CancellationToken.None);

        Assert.Equal(payment.Id, result);
        Assert.Equal(PaymentStatus.Authorized, payment.Status);
        Assert.Equal("provider-ref", payment.ProviderReference);
        Assert.Equal("auth-code", payment.AuthorizationCode);
    }

    [Fact]
    public async Task AuthorizePayment_ShouldFailPaymentWhenGatewayFails()
    {
        var payment = TestData.Payment();
        var repo = PaymentRepo(payment);
        var gateway = new Mock<IPaymentGateway>();

        gateway.Setup(x => x.AuthorizeAsync(
                It.IsAny<PaymentAuthorizationRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentAuthorizationResult(
                false, null, null, "DECLINED", "Declined"));

        var result = await CreateAuthorizeHandler(repo, gateway).Handle(
            new AuthorizePaymentCommand(payment.Id),
            CancellationToken.None);

        Assert.Equal(payment.Id, result);
        Assert.Equal(PaymentStatus.Failed, payment.Status);
        Assert.Equal("DECLINED", payment.FailureCode);
    }

    [Fact]
    public async Task CapturePayment_ShouldCaptureWhenGatewaySucceeds()
    {
        var payment = TestData.AuthorizedPayment();
        var repo = PaymentRepo(payment);
        var gateway = new Mock<IPaymentGateway>();

        gateway.Setup(x => x.CaptureAsync(
                It.IsAny<PaymentCaptureRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentCaptureResult(
                true, "capture-provider-ref", null, null));

        var result = await CreateCaptureHandler(repo, gateway).Handle(
            new CapturePaymentCommand(payment.Id),
            CancellationToken.None);

        Assert.Equal(payment.Id, result);
        Assert.Equal(PaymentStatus.Captured, payment.Status);
        Assert.Equal("capture-provider-ref", payment.ProviderReference);
    }

    [Fact]
    public async Task CapturePayment_ShouldFailPaymentWhenGatewayFails()
    {
        var payment = TestData.AuthorizedPayment();
        var repo = PaymentRepo(payment);
        var gateway = new Mock<IPaymentGateway>();

        gateway.Setup(x => x.CaptureAsync(
                It.IsAny<PaymentCaptureRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentCaptureResult(
                false, null, "CAPTURE_FAILED", "Capture failed"));

        var result = await CreateCaptureHandler(repo, gateway).Handle(
            new CapturePaymentCommand(payment.Id),
            CancellationToken.None);

        Assert.Equal(payment.Id, result);
        Assert.Equal(PaymentStatus.Failed, payment.Status);
        Assert.Equal("CAPTURE_FAILED", payment.FailureCode);
    }

    [Fact]
    public async Task RefundPayment_ShouldRefundWhenGatewaySucceeds()
    {
        var payment = TestData.CapturedPayment();
        var repo = PaymentRepo(payment);
        var gateway = new Mock<IPaymentGateway>();

        gateway.Setup(x => x.RefundAsync(
                It.IsAny<PaymentRefundRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentRefundResult(
                true, "refund-ref", null, null));

        var result = await new RefundPaymentCommandHandler(repo.Object, gateway.Object).Handle(
            new RefundPaymentCommand(payment.Id, 25m, "Customer request"),
            CancellationToken.None);

        Assert.Equal(payment.Id, result);
        Assert.Equal(25m, payment.TotalRefundedAmount);
        Assert.Equal(PaymentStatus.Captured, payment.Status);
    }

    [Fact]
    public async Task RefundPayment_ShouldThrowConflictWhenGatewayFails()
    {
        var payment = TestData.CapturedPayment();
        var repo = PaymentRepo(payment);
        var gateway = new Mock<IPaymentGateway>();

        gateway.Setup(x => x.RefundAsync(
                It.IsAny<PaymentRefundRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentRefundResult(
                false, null, "REFUND_FAILED", "Refund declined"));

        await Assert.ThrowsAsync<ConflictException>(() =>
            new RefundPaymentCommandHandler(repo.Object, gateway.Object).Handle(
                new RefundPaymentCommand(payment.Id, 25m, "Customer request"),
                CancellationToken.None));

        Assert.Equal(0m, payment.TotalRefundedAmount);
    }

    [Fact]
    public async Task CancelPayment_ShouldCancelPendingPayment()
    {
        var payment = TestData.Payment();
        var repo = PaymentRepo(payment);

        var result = await new CancelPaymentCommandHandler(repo.Object).Handle(
            new CancelPaymentCommand(payment.Id, "Customer cancelled"),
            CancellationToken.None);

        Assert.Equal(payment.Id, result);
        Assert.Equal(PaymentStatus.Cancelled, payment.Status);
    }

    [Fact]
    public async Task FailPayment_ShouldMarkPaymentFailed()
    {
        var payment = TestData.Payment();
        var repo = PaymentRepo(payment);

        var result = await new FailPaymentCommandHandler(repo.Object).Handle(
            new FailPaymentCommand(payment.Id, "DECLINED", "Provider declined"),
            CancellationToken.None);

        Assert.Equal(payment.Id, result);
        Assert.Equal(PaymentStatus.Failed, payment.Status);
        Assert.Equal("DECLINED", payment.FailureCode);
    }

    [Fact]
    public async Task SettlePayment_ShouldSettleCapturedPayment()
    {
        var payment = TestData.CapturedPayment();
        var repo = PaymentRepo(payment);

        var result = await new SettlePaymentCommandHandler(repo.Object).Handle(
            new SettlePaymentCommand(payment.Id),
            CancellationToken.None);

        Assert.Equal(payment.Id, result);
        Assert.Equal(PaymentStatus.Settled, payment.Status);
    }

    [Fact]
    public async Task GetPaymentById_ShouldReturnMappedDto()
    {
        var payment = TestData.CapturedPayment();
        var repo = PaymentRepo(payment);

        var result = await new GetPaymentByIdQueryHandler(repo.Object).Handle(
            new GetPaymentByIdQuery(payment.Id),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(payment.Id, result!.Id);
        Assert.Equal(payment.Amount, result.Amount);
        Assert.Equal(PaymentStatus.Captured, result.Status);
    }

    [Fact]
    public async Task GetPaymentById_ShouldReturnNullWhenMissing()
    {
        var id = Guid.NewGuid();
        var repo = PaymentRepo(null);

        Assert.Null(await new GetPaymentByIdQueryHandler(repo.Object).Handle(
            new GetPaymentByIdQuery(id),
            CancellationToken.None));
    }

    [Fact]
    public async Task GetPayments_ShouldReturnPagedMappedResults()
    {
        var payments = new[]
        {
            TestData.Payment(),
            TestData.Payment(50m)
        };

        var repo = new Mock<IReadRepository<Payment>>();

        repo.Setup(x => x.CountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(11);

        repo.Setup(x => x.ListAsync(10, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payments);

        var result = await new GetPaymentsQueryHandler(repo.Object).Handle(
            new GetPaymentsQuery(new PaginationRequest(2, 10)),
            CancellationToken.None);

        Assert.Equal(11, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.PageNumber);
    }

    [Fact]
    public async Task PaymentRepositoryHandlers_ShouldThrowNotFoundWhenPaymentDoesNotExist()
    {
        var id = Guid.NewGuid();
        var repo = PaymentRepo(null);
        var gateway = new Mock<IPaymentGateway>();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateAuthorizeHandler(repo, gateway).Handle(
                new AuthorizePaymentCommand(id),
                CancellationToken.None));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateCaptureHandler(repo, gateway).Handle(
                new CapturePaymentCommand(id),
                CancellationToken.None));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            new CancelPaymentCommandHandler(repo.Object).Handle(
                new CancelPaymentCommand(id, "Reason"),
                CancellationToken.None));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            new FailPaymentCommandHandler(repo.Object).Handle(
                new FailPaymentCommand(id, "CODE", "Reason"),
                CancellationToken.None));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            new RefundPaymentCommandHandler(repo.Object, gateway.Object).Handle(
                new RefundPaymentCommand(id, 10m, "Reason"),
                CancellationToken.None));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            new SettlePaymentCommandHandler(repo.Object).Handle(
                new SettlePaymentCommand(id),
                CancellationToken.None));
    }

    private static AuthorizePaymentCommandHandler CreateAuthorizeHandler(
        Mock<IPaymentRepository> paymentRepository,
        Mock<IPaymentGateway> gateway)
    {
        return new AuthorizePaymentCommandHandler(
            paymentRepository.Object,
            gateway.Object,
            new Mock<IOrderRepository>().Object,
            new Mock<IInventoryRepository>().Object);
    }

    private static CapturePaymentCommandHandler CreateCaptureHandler(
        Mock<IPaymentRepository> paymentRepository,
        Mock<IPaymentGateway> gateway)
    {
        return new CapturePaymentCommandHandler(
            paymentRepository.Object,
            gateway.Object,
            new Mock<IOrderRepository>().Object,
            new Mock<IInventoryRepository>().Object);
    }

    private static Mock<IPaymentRepository> PaymentRepo(Payment? payment)
    {
        var repo = new Mock<IPaymentRepository>();

        if (payment is null)
        {
            repo.Setup(x => x.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Payment?)null);
        }
        else
        {
            repo.Setup(x => x.GetByIdAsync(
                    payment.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(payment);
        }

        return repo;
    }
}
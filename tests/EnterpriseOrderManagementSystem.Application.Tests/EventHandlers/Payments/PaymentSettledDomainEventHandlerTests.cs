
using EnterpriseOrderManagementSystem.Application.Abstractions.Services;
using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Application.Features.Payments.EventHandlers;
using EnterpriseOrderManagementSystem.Application.Tests.Common;
using EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.PaymentAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.PaymentAggregate.Events;
using EnterpriseOrderManagementSystem.Domain.Enums;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using Moq;

namespace EnterpriseOrderManagementSystem.Application.Tests.EventHandlers.Payments;

public sealed class PaymentSettledDomainEventHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenOrderAndCustomerExist_SendsEmail()
    {
        // Arrange
        var customer = TestData.Customer();
        var warehouseId = Guid.NewGuid();
        var order = TestData.Order(customer.Id, warehouseId);

        var payment = Payment.Create(
            Guid.NewGuid(),
            order.Id,
            100m,
            "USD",
            PaymentMethod.CreditCard,
            PaymentProvider.Stripe);

        var orders = new Mock<IOrderRepository>();
        var customers = new Mock<ICustomerRepository>();
        var email = new Mock<IEmailService>();

        orders
            .Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        customers
            .Setup(x => x.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var handler = new PaymentSettledDomainEventHandler(
            orders.Object,
            customers.Object,
            email.Object);

        var evt = new PaymentSettledDomainEvent(
            payment.Id,
            order.Id,
            100m,
            "provider-ref");

        // Act
        await handler.HandleAsync(evt);

        // Assert
        email.Verify(
            x => x.SendAsync(
                customer.Email.ToString(),
                "Payment settled",
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenOrderDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        var orders = new Mock<IOrderRepository>();
        var customers = new Mock<ICustomerRepository>();
        var email = new Mock<IEmailService>();

        orders
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var handler = new PaymentSettledDomainEventHandler(
            orders.Object,
            customers.Object,
            email.Object);

        var evt = new PaymentSettledDomainEvent(
            Guid.NewGuid(),
            orderId,
            100m,
            "ref");

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(evt));

        email.Verify(
            x => x.SendAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
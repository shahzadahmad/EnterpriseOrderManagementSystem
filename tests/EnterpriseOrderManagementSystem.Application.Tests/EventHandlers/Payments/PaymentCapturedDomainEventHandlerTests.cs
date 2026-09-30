using EnterpriseOrderManagementSystem.Application.Abstractions.Services;
using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Application.EventHandlers.Payments;
using EnterpriseOrderManagementSystem.Application.Tests.Common;
using EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.PaymentAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.PaymentAggregate.Events;
using EnterpriseOrderManagementSystem.Domain.Enums;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using Moq;

namespace EnterpriseOrderManagementSystem.Application.Tests.EventHandlers.Payments;

public sealed class PaymentCapturedDomainEventHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenOrderAndCustomerExist_SendsEmail()
    {
        var customer = TestData.Customer();
        var order = Order.Create(customer.Id);
        var payment = Payment.Create(Guid.NewGuid(), order.Id, 100m, "USD", PaymentMethod.CreditCard, PaymentProvider.Stripe);
        var orders = new Mock<IOrderRepository>();
        var customers = new Mock<ICustomerRepository>();
        var email = new Mock<IEmailService>();
        orders.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        customers.Setup(x => x.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        var handler = new PaymentCapturedDomainEventHandler(orders.Object, customers.Object, email.Object);

        await handler.HandleAsync(new PaymentCapturedDomainEvent(payment.Id, order.Id, 100m, "provider-ref"));

        email.Verify(x => x.SendAsync(customer.Email.ToString(), "Payment captured", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenOrderDoesNotExist_ThrowsNotFoundException()
    {
        var orderId = Guid.NewGuid();
        var orders = new Mock<IOrderRepository>();
        var customers = new Mock<ICustomerRepository>();
        var email = new Mock<IEmailService>();
        orders.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync((Order?)null);
        var handler = new PaymentCapturedDomainEventHandler(orders.Object, customers.Object, email.Object);
        var evt = new PaymentCapturedDomainEvent(Guid.NewGuid(), orderId, 100m, "ref" );
        await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(evt));
        email.Verify(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

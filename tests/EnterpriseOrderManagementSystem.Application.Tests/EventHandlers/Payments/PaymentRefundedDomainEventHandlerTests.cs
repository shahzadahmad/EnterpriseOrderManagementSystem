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

public sealed class PaymentRefundedDomainEventHandlerTests
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
        var handler = new PaymentRefundedDomainEventHandler(orders.Object, customers.Object, email.Object);

        await handler.HandleAsync(new PaymentRefundedDomainEvent(payment.Id, order.Id, 25m, 25m, "refund-ref", "Customer request"));

        email.Verify(x => x.SendAsync(customer.Email.ToString(), "Payment refunded", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenOrderDoesNotExist_ThrowsNotFoundException()
    {
        var orderId = Guid.NewGuid();
        var orders = new Mock<IOrderRepository>();
        var customers = new Mock<ICustomerRepository>();
        var email = new Mock<IEmailService>();
        orders.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync((Order?)null);
        var handler = new PaymentRefundedDomainEventHandler(orders.Object, customers.Object, email.Object);
        var evt = new PaymentRefundedDomainEvent(Guid.NewGuid(), orderId, 25m, 25m, "ref", "reason" );
        await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(evt));
        email.Verify(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

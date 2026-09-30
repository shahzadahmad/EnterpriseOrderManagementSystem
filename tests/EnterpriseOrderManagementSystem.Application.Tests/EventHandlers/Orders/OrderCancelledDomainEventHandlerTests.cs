using EnterpriseOrderManagementSystem.Application.Abstractions.Services;
using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Application.EventHandlers.Orders;
using EnterpriseOrderManagementSystem.Application.Tests.Common;
using EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate.Events;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using Moq;

namespace EnterpriseOrderManagementSystem.Application.Tests.EventHandlers.Orders;

public sealed class OrderCancelledDomainEventHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenOrderAndCustomerExist_SendsEmail()
    {
        var customer = TestData.Customer();
        var order = Order.Create(customer.Id);
        var orders = new Mock<IOrderRepository>();
        var customers = new Mock<ICustomerRepository>();
        var email = new Mock<IEmailService>();
        orders.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        customers.Setup(x => x.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        var handler = new OrderCancelledDomainEventHandler(orders.Object, customers.Object, email.Object);

        await handler.HandleAsync(new OrderCancelledDomainEvent(order.Id, "Customer request"));

        email.Verify(x => x.SendAsync(customer.Email.ToString(), "Order cancelled", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenOrderDoesNotExist_ThrowsNotFoundException()
    {
        var orderId = Guid.NewGuid();
        var orders = new Mock<IOrderRepository>();
        var customers = new Mock<ICustomerRepository>();
        var email = new Mock<IEmailService>();
        orders.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync((Order?)null);
        var handler = new OrderCancelledDomainEventHandler(orders.Object, customers.Object, email.Object);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(new OrderCancelledDomainEvent(orderId, "reason")));
        Assert.Contains("Order", exception.Message);
        email.Verify(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

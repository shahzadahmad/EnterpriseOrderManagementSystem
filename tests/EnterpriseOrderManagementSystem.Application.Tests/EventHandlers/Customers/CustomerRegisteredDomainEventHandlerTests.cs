using EnterpriseOrderManagementSystem.Application.Abstractions.Services;
using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Application.EventHandlers.Customers;
using EnterpriseOrderManagementSystem.Application.Tests.Common;
using EnterpriseOrderManagementSystem.Domain.Aggregates.CustomerAggregate.Events;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using Moq;


namespace EnterpriseOrderManagementSystem.Application.Tests.EventHandlers.Customers;

public sealed class CustomerRegisteredDomainEventHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenCustomerExists_SendsExpectedEmail()
    {
        var customer = TestData.Customer();
        var repository = new Mock<ICustomerRepository>();
        var email = new Mock<IEmailService>();
        repository.Setup(x => x.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        var handler = new CustomerRegisteredDomainEventHandler(repository.Object, email.Object);

        await handler.HandleAsync(new CustomerRegisteredDomainEvent(customer.Id));

        email.Verify(x => x.SendAsync(customer.Email.ToString(), "Welcome to Enterprise Order Management", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        repository.Verify(x => x.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenCustomerDoesNotExist_ThrowsNotFoundException()
    {
        var id = Guid.NewGuid();
        var repository = new Mock<ICustomerRepository>();
        var email = new Mock<IEmailService>();
        repository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((EnterpriseOrderManagementSystem.Domain.Aggregates.CustomerAggregate.Customer?)null);
        var handler = new CustomerRegisteredDomainEventHandler(repository.Object, email.Object);

        await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(new CustomerRegisteredDomainEvent(id)));
        email.Verify(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

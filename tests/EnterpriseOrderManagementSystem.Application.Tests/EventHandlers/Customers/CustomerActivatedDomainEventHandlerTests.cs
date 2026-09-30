using EnterpriseOrderManagementSystem.Application.Abstractions.Services;
using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Application.EventHandlers.Customers;
using EnterpriseOrderManagementSystem.Application.Tests.Common;
using EnterpriseOrderManagementSystem.Domain.Aggregates.CustomerAggregate.Events;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using Moq;


namespace EnterpriseOrderManagementSystem.Application.Tests.EventHandlers.Customers;

public sealed class CustomerActivatedDomainEventHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenCustomerExists_SendsExpectedEmail()
    {
        var customer = TestData.Customer();
        var repository = new Mock<ICustomerRepository>();
        var email = new Mock<IEmailService>();
        repository.Setup(x => x.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        var handler = new CustomerActivatedDomainEventHandler(repository.Object, email.Object);

        await handler.HandleAsync(new CustomerActivatedDomainEvent(customer.Id));

        email.Verify(x => x.SendAsync(customer.Email.ToString(), "Customer account activated", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        repository.Verify(x => x.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenCustomerDoesNotExist_ThrowsNotFoundException()
    {
        var id = Guid.NewGuid();
        var repository = new Mock<ICustomerRepository>();
        var email = new Mock<IEmailService>();
        repository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((EnterpriseOrderManagementSystem.Domain.Aggregates.CustomerAggregate.Customer?)null);
        var handler = new CustomerActivatedDomainEventHandler(repository.Object, email.Object);

        await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(new CustomerActivatedDomainEvent(id)));
        email.Verify(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Application.Features.Customers.Commands;
using EnterpriseOrderManagementSystem.Application.Features.Customers.Queries;
using EnterpriseOrderManagementSystem.Application.Abstractions.Persistence;
using EnterpriseOrderManagementSystem.Application.Tests.Common;
using EnterpriseOrderManagementSystem.Domain.Aggregates.CustomerAggregate;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using Moq;

namespace EnterpriseOrderManagementSystem.Application.Tests.Features.Customers;

public sealed class CustomerHandlerTests
{
    [Fact]
    public async Task CreateCustomer_ShouldCreateAndPersistCustomer()
    {
        var repository = new Mock<ICustomerRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        Customer? added = null;
        repository.Setup(x => x.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
            .Callback<Customer, CancellationToken>((c, _) => added = c)
            .Returns(Task.CompletedTask);

        var handler = new CreateCustomerCommandHandler(repository.Object, unitOfWork.Object);
        var result = await handler.Handle(new CreateCustomerCommand(
            " John ", " Doe ", "john@example.com", "+92", "3001234567",
            "Street", "Islamabad", "ICT", "Pakistan", "44000"), CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result);
        Assert.NotNull(added);
        Assert.Equal(result, added!.Id);
        Assert.Equal("John", added.FirstName);
        repository.Verify(x => x.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ActivateCustomer_ShouldActivateCustomer()
    {
        var customer = TestData.Customer();
        customer.Deactivate();
        var repository = new Mock<ICustomerRepository>();
        repository.Setup(x => x.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);

        var handler = new ActivateCustomerCommandHandler(repository.Object);
        var result = await handler.Handle(new ActivateCustomerCommand(customer.Id), CancellationToken.None);

        Assert.Equal(customer.Id, result);
        Assert.Equal(Domain.Enums.CustomerStatus.Active, customer.Status);
    }

    [Fact]
    public async Task ActivateCustomer_ShouldThrowWhenCustomerDoesNotExist()
    {
        var id = Guid.NewGuid();
        var repository = new Mock<ICustomerRepository>();
        repository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Customer?)null);

        var handler = new ActivateCustomerCommandHandler(repository.Object);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new ActivateCustomerCommand(id), CancellationToken.None));
    }

    [Fact]
    public async Task DeactivateCustomer_ShouldDeactivateCustomer()
    {
        var customer = TestData.Customer();
        var repository = new Mock<ICustomerRepository>();
        repository.Setup(x => x.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);

        var handler = new DeactivateCustomerCommandHandler(repository.Object);
        var result = await handler.Handle(new DeactivateCustomerCommand(customer.Id), CancellationToken.None);

        Assert.Equal(customer.Id, result);
        Assert.Equal(Domain.Enums.CustomerStatus.Inactive, customer.Status);
    }

    [Fact]
    public async Task DeactivateCustomer_ShouldThrowWhenCustomerDoesNotExist()
    {
        var id = Guid.NewGuid();
        var repository = new Mock<ICustomerRepository>();
        repository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Customer?)null);

        var handler = new DeactivateCustomerCommandHandler(repository.Object);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new DeactivateCustomerCommand(id), CancellationToken.None));
    }

    [Fact]
    public async Task GetCustomerById_ShouldReturnMappedDto()
    {
        var customer = TestData.Customer();
        var repository = new Mock<ICustomerRepository>();
        repository.Setup(x => x.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);

        var handler = new GetCustomerByIdQueryHandler(repository.Object);
        var result = await handler.Handle(new GetCustomerByIdQuery(customer.Id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(customer.Id, result!.Id);
        Assert.Equal(customer.Email.Value, result.Email);
    }

    [Fact]
    public async Task GetCustomerById_ShouldReturnNullWhenMissing()
    {
        var id = Guid.NewGuid();
        var repository = new Mock<ICustomerRepository>();
        repository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Customer?)null);

        var handler = new GetCustomerByIdQueryHandler(repository.Object);

        Assert.Null(await handler.Handle(new GetCustomerByIdQuery(id), CancellationToken.None));
    }

    [Fact]
    public async Task GetCustomers_ShouldReturnPagedMappedResults()
    {
        var customers = new[] { TestData.Customer(), TestData.Customer() };
        var repository = new Mock<IReadRepository<Customer>>();
        repository.Setup(x => x.CountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(25);
        repository.Setup(x => x.ListAsync(10, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customers);

        var handler = new GetCustomersQueryHandler(repository.Object);
        var result = await handler.Handle(new GetCustomersQuery(
            new Application.Common.Models.PaginationRequest(2, 10)), CancellationToken.None);

        Assert.Equal(2, result.PageNumber);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(25, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        repository.Verify(x => x.ListAsync(10, 10, It.IsAny<CancellationToken>()), Times.Once);
    }
}

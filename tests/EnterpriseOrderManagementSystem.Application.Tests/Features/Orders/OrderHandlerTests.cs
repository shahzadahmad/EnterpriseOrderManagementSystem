using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Application.Common.Models;
using EnterpriseOrderManagementSystem.Application.Features.Orders.Commands;
using EnterpriseOrderManagementSystem.Application.Features.Orders.Queries;
using EnterpriseOrderManagementSystem.Application.Abstractions.Persistence;
using EnterpriseOrderManagementSystem.Application.Tests.Common;
using EnterpriseOrderManagementSystem.Domain.Aggregates.CustomerAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.ProductAggregate;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using Moq;

namespace EnterpriseOrderManagementSystem.Application.Tests.Features.Orders;

public sealed class OrderHandlerTests
{
    [Fact]
    public async Task CreateOrder_ShouldCreateAndPersistOrder()
    {
        var customer = TestData.Customer();
        var orders = new Mock<IOrderRepository>();
        var customers = new Mock<ICustomerRepository>();
        customers.Setup(x => x.ExistsAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        Order? added = null;
        orders.Setup(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((o, _) => added = o).Returns(Task.CompletedTask);

        var result = await new CreateOrderCommandHandler(orders.Object, customers.Object).Handle(
            new CreateOrderCommand(customer.Id), CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result);
        Assert.Equal(result, added!.Id);
        Assert.Equal(customer.Id, added.CustomerId);
    }

    [Fact]
    public async Task CreateOrder_ShouldThrowWhenCustomerDoesNotExist()
    {
        var id = Guid.NewGuid();
        var orders = new Mock<IOrderRepository>();
        var customers = new Mock<ICustomerRepository>();
        customers.Setup(x => x.ExistsAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            new CreateOrderCommandHandler(orders.Object, customers.Object).Handle(
                new CreateOrderCommand(id), CancellationToken.None));
        orders.Verify(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddOrderItem_ShouldAddProductToOrder()
    {
        var order = TestData.Order();
        var product = TestData.Product(25m);
        var orders = OrderRepo(order);
        var products = new Mock<IProductRepository>();
        products.Setup(x => x.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var result = await new AddOrderItemCommandHandler(orders.Object, products.Object).Handle(
            new AddOrderItemCommand(order.Id, product.Id, 2), CancellationToken.None);

        Assert.Equal(order.Id, result);
        Assert.Single(order.Items);
        Assert.Equal(50m, order.TotalAmount.Amount);
    }

    [Fact]
    public async Task AddOrderItem_ShouldThrowWhenOrderDoesNotExist()
    {
        var id = Guid.NewGuid();
        var orders = OrderRepo(null);
        var products = new Mock<IProductRepository>();
        await Assert.ThrowsAsync<NotFoundException>(() =>
            new AddOrderItemCommandHandler(orders.Object, products.Object).Handle(
                new AddOrderItemCommand(id, Guid.NewGuid(), 1), CancellationToken.None));
    }

    [Fact]
    public async Task AddOrderItem_ShouldThrowWhenProductDoesNotExist()
    {
        var order = TestData.Order();
        var orders = OrderRepo(order);
        var products = new Mock<IProductRepository>();
        products.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            new AddOrderItemCommandHandler(orders.Object, products.Object).Handle(
                new AddOrderItemCommand(order.Id, Guid.NewGuid(), 1), CancellationToken.None));
    }

    [Fact]
    public async Task ConfirmOrder_ShouldConfirmOrder()
    {
        var order = TestData.OrderWithItem();
        var repo = OrderRepo(order);
        var result = await new ConfirmOrderCommandHandler(repo.Object).Handle(
            new ConfirmOrderCommand(order.Id), CancellationToken.None);
        Assert.Equal(order.Id, result);
        Assert.Equal(Domain.Enums.OrderStatus.Confirmed, order.Status);
    }

    [Fact]
    public async Task ShipOrder_ShouldShipConfirmedOrder()
    {
        var order = TestData.ConfirmedOrder();
        var repo = OrderRepo(order);
        var result = await new ShipOrderCommandHandler(repo.Object).Handle(
            new ShipOrderCommand(order.Id), CancellationToken.None);
        Assert.Equal(order.Id, result);
        Assert.Equal(Domain.Enums.OrderStatus.Shipped, order.Status);
    }

    [Fact]
    public async Task DeliverOrder_ShouldDeliverShippedOrder()
    {
        var order = TestData.ShippedOrder();
        var repo = OrderRepo(order);
        var result = await new DeliverOrderCommandHandler(repo.Object).Handle(
            new DeliverOrderCommand(order.Id), CancellationToken.None);
        Assert.Equal(order.Id, result);
        Assert.Equal(Domain.Enums.OrderStatus.Delivered, order.Status);
    }

    [Fact]
    public async Task CancelOrder_ShouldCancelPendingOrder()
    {
        var order = TestData.Order();
        var repo = OrderRepo(order);
        var result = await new CancelOrderCommandHandler(repo.Object).Handle(
            new CancelOrderCommand(order.Id, "Customer requested cancellation"), CancellationToken.None);
        Assert.Equal(order.Id, result);
        Assert.Equal(Domain.Enums.OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public async Task GetOrderById_ShouldReturnMappedDto()
    {
        var order = TestData.OrderWithItem(20m, 2);
        var repo = OrderRepo(order);
        var result = await new GetOrderByIdQueryHandler(repo.Object).Handle(
            new GetOrderByIdQuery(order.Id), CancellationToken.None);
        Assert.NotNull(result);
        Assert.Equal(order.Id, result!.Id);
        Assert.Equal(40m, result.TotalAmount);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task GetOrderById_ShouldReturnNullWhenMissing()
    {
        var id = Guid.NewGuid();
        var repo = OrderRepo(null);
        Assert.Null(await new GetOrderByIdQueryHandler(repo.Object).Handle(
            new GetOrderByIdQuery(id), CancellationToken.None));
    }

    [Fact]
    public async Task GetOrders_ShouldReturnPagedMappedResults()
    {
        var orders = new[] { TestData.Order(), TestData.OrderWithItem() };
        var repo = new Mock<IReadRepository<Order>>();
        repo.Setup(x => x.CountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(12);
        repo.Setup(x => x.ListAsync(10, 10, It.IsAny<CancellationToken>())).ReturnsAsync(orders);

        var result = await new GetOrdersQueryHandler(repo.Object).Handle(
            new GetOrdersQuery(new PaginationRequest(2, 10)), CancellationToken.None);

        Assert.Equal(12, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.PageNumber);
    }

    [Fact]
    public async Task OrderStateCommands_ShouldThrowNotFoundWhenOrderDoesNotExist()
    {
        var id = Guid.NewGuid();
        var repo = OrderRepo(null);
        await Assert.ThrowsAsync<NotFoundException>(() => new ConfirmOrderCommandHandler(repo.Object)
            .Handle(new ConfirmOrderCommand(id), CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(() => new ShipOrderCommandHandler(repo.Object)
            .Handle(new ShipOrderCommand(id), CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(() => new DeliverOrderCommandHandler(repo.Object)
            .Handle(new DeliverOrderCommand(id), CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(() => new CancelOrderCommandHandler(repo.Object)
            .Handle(new CancelOrderCommand(id, "Reason"), CancellationToken.None));
    }

    private static Mock<IOrderRepository> OrderRepo(Order? order)
    {
        var repo = new Mock<IOrderRepository>();
        if (order is null)
            repo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Order?)null);
        else
            repo.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        return repo;
    }
}

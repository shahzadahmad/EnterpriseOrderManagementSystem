
using EnterpriseOrderManagementSystem.Application.Abstractions.Persistence;
using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Application.Common.Models;
using EnterpriseOrderManagementSystem.Application.Features.Orders.Commands;
using EnterpriseOrderManagementSystem.Application.Features.Orders.Queries;
using EnterpriseOrderManagementSystem.Application.Tests.Common;
using EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.ProductAggregate;
using EnterpriseOrderManagementSystem.Domain.Enums;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using Moq;

using InvtAggregate = EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate;

namespace EnterpriseOrderManagementSystem.Application.Tests.Features.Orders;

public sealed class OrderHandlerTests
{
    [Fact]
    public async Task CreateOrder_ShouldCreateAndPersistOrder()
    {
        // Arrange
        var customer = TestData.Customer();
        var warehouseId = Guid.NewGuid();

        var orders = new Mock<IOrderRepository>();
        var customers = new Mock<ICustomerRepository>();

        customers
            .Setup(x => x.ExistsAsync(customer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Order? addedOrder = null;

        orders
            .Setup(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((order, _) => addedOrder = order)
            .Returns(Task.CompletedTask);

        var handler = new CreateOrderCommandHandler(
            orders.Object,
            customers.Object);

        // Act
        var result = await handler.Handle(
            new CreateOrderCommand(customer.Id, warehouseId),
            CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        Assert.NotNull(addedOrder);
        Assert.Equal(result, addedOrder.Id);
        Assert.Equal(customer.Id, addedOrder.CustomerId);
        Assert.Equal(warehouseId, addedOrder.WarehouseId);
    }

    [Fact]
    public async Task CreateOrder_ShouldThrowWhenCustomerDoesNotExist()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();

        var orders = new Mock<IOrderRepository>();
        var customers = new Mock<ICustomerRepository>();

        customers
            .Setup(x => x.ExistsAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new CreateOrderCommandHandler(
            orders.Object,
            customers.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new CreateOrderCommand(customerId, warehouseId),
                CancellationToken.None));

        orders.Verify(
            x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task AddOrderItem_ShouldAddProductToOrder()
    {
        // Arrange
        var order = TestData.Order();
        var product = TestData.Product(25m);

        var orders = OrderRepo(order);
        var products = new Mock<IProductRepository>();

        products
            .Setup(x => x.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var handler = new AddOrderItemCommandHandler(
            orders.Object,
            products.Object);

        // Act
        var result = await handler.Handle(
            new AddOrderItemCommand(order.Id, product.Id, 2),
            CancellationToken.None);

        // Assert
        Assert.Equal(order.Id, result);
        Assert.Single(order.Items);
        Assert.Equal(50m, order.TotalAmount.Amount);
    }

    [Fact]
    public async Task AddOrderItem_ShouldThrowWhenOrderDoesNotExist()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        var orders = OrderRepo(null);
        var products = new Mock<IProductRepository>();

        var handler = new AddOrderItemCommandHandler(
            orders.Object,
            products.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new AddOrderItemCommand(orderId, Guid.NewGuid(), 1),
                CancellationToken.None));
    }

    [Fact]
    public async Task AddOrderItem_ShouldThrowWhenProductDoesNotExist()
    {
        // Arrange
        var order = TestData.Order();

        var orders = OrderRepo(order);
        var products = new Mock<IProductRepository>();

        products
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var handler = new AddOrderItemCommandHandler(
            orders.Object,
            products.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new AddOrderItemCommand(order.Id, Guid.NewGuid(), 1),
                CancellationToken.None));
    }

    [Fact]
    public async Task ConfirmOrder_ShouldConfirmOrder()
    {
        // Arrange
        var order = TestData.OrderWithItem();
        var item = Assert.Single(order.Items);

        var orders = OrderRepo(order);
        var inventory = new Mock<IInventoryRepository>();

        var inventoryRecord = InvtAggregate.Inventory.Create(
            Guid.NewGuid(),
            item.ProductId,
            order.WarehouseId,
            5,
            1000);

        inventoryRecord.ReceiveStock(
            item.Quantity,
            "TEST-STOCK-RECEIPT-001",
            "Initial stock received for order confirmation test");

        inventory
            .Setup(x => x.GetByProductAndWarehouseAsync(
                item.ProductId,
                order.WarehouseId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventoryRecord);

        var handler = new ConfirmOrderCommandHandler(
            orders.Object,
            inventory.Object);

        // Act
        var result = await handler.Handle(
            new ConfirmOrderCommand(order.Id),
            CancellationToken.None);

        // Assert
        Assert.Equal(order.Id, result);
        Assert.Equal(OrderStatus.Confirmed, order.Status);

        inventory.Verify(
            x => x.GetByProductAndWarehouseAsync(
                item.ProductId,
                order.WarehouseId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ShipOrder_ShouldShipConfirmedOrder()
    {
        // Arrange
        var order = TestData.ConfirmedOrder();
        var item = Assert.Single(order.Items);

        var orders = OrderRepo(order);
        var inventory = new Mock<IInventoryRepository>();
        var payments = new Mock<IPaymentRepository>();

        var inventoryRecord = InvtAggregate.Inventory.Create(
            Guid.NewGuid(),
            item.ProductId,
            order.WarehouseId,
            5,
            1000);

        inventoryRecord.ReceiveStock(
            item.Quantity,
            "TEST-STOCK-RECEIPT-002",
            "Initial stock for order shipment test");

        inventoryRecord.ReserveStock(
            item.Quantity,
            order.OrderNumber,
            $"Stock reserved for order {order.OrderNumber}");

        inventory
            .Setup(x => x.GetByProductAndWarehouseAsync(
                item.ProductId,
                order.WarehouseId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventoryRecord);

        // A captured payment is required before shipment.
        // Mock the repository to return a successfully captured payment.
        payments
            .Setup(x => x.GetByOrderIdAsync(
                order.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.CapturedPayment());

        var handler = new ShipOrderCommandHandler(
            orders.Object,
            inventory.Object,
            payments.Object);

        // Act
        var result = await handler.Handle(
            new ShipOrderCommand(order.Id),
            CancellationToken.None);

        // Assert
        Assert.Equal(order.Id, result);
        Assert.Equal(OrderStatus.Shipped, order.Status);

        Assert.Equal(0, inventoryRecord.ReservedQuantity);
        Assert.Equal(0, inventoryRecord.AvailableQuantity);

        payments.Verify(
            x => x.GetByOrderIdAsync(
                order.Id,
                It.IsAny<CancellationToken>()),
            Times.Once);

        inventory.Verify(
            x => x.GetByProductAndWarehouseAsync(
                item.ProductId,
                order.WarehouseId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeliverOrder_ShouldDeliverShippedOrder()
    {
        // Arrange
        var order = TestData.ShippedOrder();
        var orders = OrderRepo(order);

        var handler = new DeliverOrderCommandHandler(orders.Object);

        // Act
        var result = await handler.Handle(
            new DeliverOrderCommand(order.Id),
            CancellationToken.None);

        // Assert
        Assert.Equal(order.Id, result);
        Assert.Equal(OrderStatus.Delivered, order.Status);
    }

    [Fact]
    public async Task CancelOrder_ShouldCancelPendingOrder()
    {
        // Arrange
        var order = TestData.Order();

        var orders = OrderRepo(order);
        var inventory = new Mock<IInventoryRepository>();
        var payments = new Mock<IPaymentRepository>();

        var handler = new CancelOrderCommandHandler(
            orders.Object,
            inventory.Object,
            payments.Object);

        // Act
        var result = await handler.Handle(
            new CancelOrderCommand(
                order.Id,
                "Customer requested cancellation"),
            CancellationToken.None);

        // Assert
        Assert.Equal(order.Id, result);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public async Task GetOrderById_ShouldReturnMappedDto()
    {
        // Arrange
        var order = TestData.OrderWithItem(20m, 2);
        var orders = OrderRepo(order);

        var handler = new GetOrderByIdQueryHandler(orders.Object);

        // Act
        var result = await handler.Handle(
            new GetOrderByIdQuery(order.Id),
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(order.Id, result!.Id);
        Assert.Equal(40m, result.TotalAmount);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task GetOrderById_ShouldReturnNullWhenMissing()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orders = OrderRepo(null);

        var handler = new GetOrderByIdQueryHandler(orders.Object);

        // Act
        var result = await handler.Handle(
            new GetOrderByIdQuery(orderId),
            CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetOrders_ShouldReturnPagedMappedResults()
    {
        // Arrange
        var orders = new[]
        {
            TestData.Order(),
            TestData.OrderWithItem()
        };

        var repository = new Mock<IReadRepository<Order>>();

        repository
            .Setup(x => x.CountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(12);

        repository
            .Setup(x => x.ListAsync(10, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        var handler = new GetOrdersQueryHandler(repository.Object);

        // Act
        var result = await handler.Handle(
            new GetOrdersQuery(new PaginationRequest(2, 10)),
            CancellationToken.None);

        // Assert
        Assert.Equal(12, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.PageNumber);
    }

    [Fact]
    public async Task OrderStateCommands_ShouldThrowNotFoundWhenOrderDoesNotExist()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        var orders = OrderRepo(null);
        var inventory = new Mock<IInventoryRepository>();
        var payments = new Mock<IPaymentRepository>();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            new ConfirmOrderCommandHandler(
                orders.Object,
                inventory.Object)
                .Handle(
                    new ConfirmOrderCommand(orderId),
                    CancellationToken.None));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            new ShipOrderCommandHandler(
                orders.Object,
                inventory.Object,
                payments.Object)
                .Handle(
                    new ShipOrderCommand(orderId),
                    CancellationToken.None));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            new DeliverOrderCommandHandler(orders.Object)
                .Handle(
                    new DeliverOrderCommand(orderId),
                    CancellationToken.None));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            new CancelOrderCommandHandler(
                orders.Object,
                inventory.Object,
                payments.Object)
                .Handle(
                    new CancelOrderCommand(orderId, "Reason"),
                    CancellationToken.None));
    }

    private static Mock<IOrderRepository> OrderRepo(Order? order)
    {
        var repo = new Mock<IOrderRepository>();

        if (order is null)
        {
            repo
                .Setup(x => x.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Order?)null);
        }
        else
        {
            repo
                .Setup(x => x.GetByIdAsync(
                    order.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);
        }

        return repo;
    }
}
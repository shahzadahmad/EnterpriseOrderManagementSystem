using EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate.Events;
using EnterpriseOrderManagementSystem.Domain.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Common.ValueObjects;
using EnterpriseOrderManagementSystem.Domain.Enums;
using FluentAssertions;

namespace EnterpriseOrderManagementSystem.Domain.Tests.Aggregates.OrderAggregate;

public sealed class OrderTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidData_CreatesOrder()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var order = Order.Create(customerId);

        // Assert
        order.Should().NotBeNull();

        order.Id.Should().NotBe(Guid.Empty);

        order.CustomerId.Should().Be(customerId);
    }

    [Fact]
    public void Create_WithValidData_GeneratesUniqueOrderId()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var order1 = Order.Create(customerId);
        var order2 = Order.Create(customerId);

        // Assert
        order1.Id.Should().NotBe(Guid.Empty);
        order2.Id.Should().NotBe(Guid.Empty);

        order1.Id.Should().NotBe(order2.Id);
    }

    [Fact]
    public void Create_WithEmptyCustomerId_ThrowsBusinessRuleViolationException()
    {
        // Arrange
        // No valid CustomerId is provided.

        // Act
        var action = () =>
            Order.Create(Guid.Empty);

        // Assert
        action.Should()
            .Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Create_WithValidData_RaisesOrderCreatedDomainEvent()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var order = Order.Create(customerId);

        // Assert
        order.DomainEvents
            .Should()
            .ContainSingle(eventItem =>
                eventItem is OrderCreatedDomainEvent);
    }

    [Fact]
    public void Create_WithValidData_OrderCreatedEventContainsGeneratedOrderId()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var order = Order.Create(customerId);

        // Assert
        var domainEvent = order.DomainEvents
            .OfType<OrderCreatedDomainEvent>()
            .Single();

        domainEvent.OrderId
            .Should()
            .Be(order.Id);
    }

    #endregion

    #region Order Item Tests

    [Fact]
    public void AddItem_WithValidData_AddsOrderItem()
    {
        // Arrange
        var order = CreateValidOrder();

        var productId = Guid.NewGuid();

        // Act
        order.AddItem(
            productId,
            productName: "Product1",
            unitPrice: Money.Create(100m, "USD"),
            quantity: 2);

        // Assert
        order.Items.Should().ContainSingle();

        order.Items
            .First()
            .ProductId
            .Should()        
            .Be(productId);        

        order.Items
            .First()
            .Quantity
            .Should()
            .Be(2);


        order.Items
            .First()
            .UnitPrice
            .Should()
            .Be(Money.Create(100m, "USD"));
        
    }

    [Fact]
    public void AddItem_WithZeroQuantity_ThrowsBusinessRuleViolationException()
    {
        // Arrange
        var order = CreateValidOrder();

        // Act
        var action = () =>
            order.AddItem(
                Guid.NewGuid(),
                productName: "Product1",
                quantity: 0,
                unitPrice: Money.Create(100m, "USD"));

        // Assert
        action.Should()
            .Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void AddItem_WithNegativeQuantity_ThrowsBusinessRuleViolationException()
    {
        // Arrange
        var order = CreateValidOrder();

        // Act
        var action = () =>
            order.AddItem(
                Guid.NewGuid(),
                productName: "Product1",
                quantity: -1,
                unitPrice: Money.Create(100m, "USD"));

        // Assert
        action.Should()
            .Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void AddItem_WithNegativeUnitPrice_ThrowsBusinessRuleViolationException()
    {
        // Arrange
        var order = CreateValidOrder();

        // Act
        var action = () =>
            order.AddItem(
                Guid.NewGuid(),
                productName: "Product1",
                quantity: 1,
                unitPrice: Money.Create(-10m, "USD"));

        // Assert
        action.Should()
            .Throw<BusinessRuleViolationException>();
    }

    #endregion

    #region Confirm Tests

    [Fact]
    public void Confirm_WhenOrderIsPending_ChangesStatusToConfirmed()
    {
        // Arrange
        var order = CreateValidOrder();

        AddValidOrderItem(order);

        // Act
        order.Confirm();

        // Assert
        order.Status
            .Should()
            .Be(OrderStatus.Confirmed);
    }

    [Fact]
    public void Confirm_WhenOrderIsPending_RaisesOrderConfirmedDomainEvent()
    {
        // Arrange
        var order = CreateValidOrder();

        AddValidOrderItem(order);

        // Act
        order.Confirm();

        // Assert
        order.DomainEvents
            .Should()
            .ContainSingle(eventItem =>
                eventItem is OrderConfirmedDomainEvent);
    }

    [Fact]
    public void Confirm_WhenOrderIsAlreadyConfirmed_ThrowsInvalidOrderStateException()
    {
        // Arrange
        var order = CreateValidOrder();

        AddValidOrderItem(order);

        // First confirmation should succeed
        order.Confirm();

        // Act
        var action = () => order.Confirm();

        // Assert
        action.Should()
            .Throw<InvalidOrderStateException>();
    }

    #endregion

    #region Cancel Tests

    [Fact]
    public void Cancel_WhenOrderCanBeCancelled_ChangesStatusToCancelled()
    {
        // Arrange
        var order = CreateValidOrder();

        // Act
        order.Cancel("Customer requested cancellation.");

        // Assert
        order.Status
            .Should()
            .Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenOrderCanBeCancelled_RaisesOrderCancelledDomainEvent()
    {
        // Arrange
        var order = CreateValidOrder();

        // Act
        order.Cancel("Customer requested cancellation.");

        // Assert
        order.DomainEvents
            .Should()
            .ContainSingle(eventItem =>
                eventItem is OrderCancelledDomainEvent);
    }

    #endregion

    #region Ship Tests

    [Fact]
    public void Ship_WhenOrderIsConfirmed_ChangesStatusToShipped()
    {
        // Arrange
        var order = CreateValidOrder();
        
        AddValidOrderItem(order);

        order.Confirm();

        // Act
        order.Ship();

        // Assert
        order.Status
            .Should()
            .Be(OrderStatus.Shipped);
    }

    [Fact]
    public void Ship_WhenOrderIsConfirmed_RaisesOrderShippedDomainEvent()
    {
        // Arrange
        var order = CreateValidOrder();

        AddValidOrderItem(order);

        order.Confirm();

        // Act
        order.Ship();

        // Assert
        order.DomainEvents
            .Should()
            .ContainSingle(eventItem =>
                eventItem is OrderShippedDomainEvent);
    }

    #endregion

    #region Deliver Tests

    [Fact]
    public void Deliver_WhenOrderIsShipped_ChangesStatusToDelivered()
    {
        // Arrange
        var order = CreateValidOrder();

        AddValidOrderItem(order);

        order.Confirm();
        order.Ship();

        // Act
        order.Deliver();

        // Assert
        order.Status
            .Should()
            .Be(OrderStatus.Delivered);
    }

    [Fact]
    public void Deliver_WhenOrderIsShipped_RaisesOrderDeliveredDomainEvent()
    {
        // Arrange
        var order = CreateValidOrder();

        AddValidOrderItem(order);

        order.Confirm();
        order.Ship();

        // Act
        order.Deliver();

        // Assert
        order.DomainEvents
            .Should()
            .ContainSingle(eventItem =>
                eventItem is OrderDeliveredDomainEvent);
    }

    #endregion

    #region Test Helpers

    private static Order CreateValidOrder()
    {
        return Order.Create(
            Guid.NewGuid());
    }

    private static void AddValidOrderItem(Order order)
    {
        order.AddItem(
            Guid.NewGuid(),
            "Test Product",
            Money.Create(100m, "USD"),
            1);
    }

    #endregion
}
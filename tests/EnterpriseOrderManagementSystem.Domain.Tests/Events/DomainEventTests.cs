using EnterpriseOrderManagementSystem.Domain.Aggregates.CustomerAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.CustomerAggregate.Events;
using EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate.Events;
using EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate.Events;
using EnterpriseOrderManagementSystem.Domain.Aggregates.PaymentAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.PaymentAggregate.Events;
using EnterpriseOrderManagementSystem.Domain.Aggregates.ProductAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.ProductAggregate.Events;
using EnterpriseOrderManagementSystem.Domain.Common.Events;
using EnterpriseOrderManagementSystem.Domain.Common.ValueObjects;
using EnterpriseOrderManagementSystem.Domain.Enums;
using Xunit;

namespace EnterpriseOrderManagementSystem.Domain.Tests.Events;

public sealed class DomainEventBaseTests
{
    [Fact]
    public void DomainEvent_ShouldGenerateUniqueEventId()
    {
        // Arrange
        var event1 = new TestDomainEvent();
        var event2 = new TestDomainEvent();

        // Assert
        Assert.NotEqual(
            Guid.Empty,
            event1.EventId);

        Assert.NotEqual(
            Guid.Empty,
            event2.EventId);

        Assert.NotEqual(
            event1.EventId,
            event2.EventId);
    }

    [Fact]
    public void DomainEvent_ShouldSetOccurredOnUtc()
    {
        // Arrange
        var before = DateTime.UtcNow;

        var domainEvent = new TestDomainEvent();

        var after = DateTime.UtcNow;

        // Assert
        Assert.InRange(
            domainEvent.OccurredOnUtc,
            before,
            after);
    }

    [Fact]
    public void DomainEvent_ShouldImplementIDomainEvent()
    {
        // Arrange
        var domainEvent = new TestDomainEvent();

        // Act
        IDomainEvent result = domainEvent;

        // Assert
        Assert.NotNull(result);
    }

    private sealed record TestDomainEvent
        : DomainEvent;
}


public sealed class CustomerDomainEventTests
{
    [Fact]
    public void CustomerCreated_ShouldRaiseCustomerRegisteredDomainEvent()
    {
        // Arrange
        var customer = CreateCustomer();

        // Act
        var domainEvent =
            Assert.Single(customer.DomainEvents);

        // Assert
        var registeredEvent =
            Assert.IsType<CustomerRegisteredDomainEvent>(
                domainEvent);

        Assert.Equal(
            customer.Id,
            registeredEvent.CustomerId);
    }

    [Fact]
    public void CustomerActivate_ShouldRaiseCustomerActivatedDomainEvent()
    {
        // Arrange
        var customer = CreateCustomer();

        customer.Deactivate();

        customer.ClearDomainEvents();

        // Act
        customer.Activate();

        // Assert
        var domainEvent =
            Assert.Single(customer.DomainEvents);

        var activatedEvent =
            Assert.IsType<CustomerActivatedDomainEvent>(
                domainEvent);

        Assert.Equal(
            customer.Id,
            activatedEvent.CustomerId);
    }

    [Fact]
    public void CustomerDeactivate_ShouldRaiseCustomerDeactivatedDomainEvent()
    {
        // Arrange
        var customer = CreateCustomer();

        customer.ClearDomainEvents();

        // Act
        customer.Deactivate();

        // Assert
        var domainEvent =
            Assert.Single(customer.DomainEvents);

        var deactivatedEvent =
            Assert.IsType<CustomerDeactivatedDomainEvent>(
                domainEvent);

        Assert.Equal(
            customer.Id,
            deactivatedEvent.CustomerId);
    }

    [Fact]
    public void CustomerDomainEvents_ShouldHaveValidMetadata()
    {
        // Arrange
        var customer = CreateCustomer();

        // Act
        var domainEvent =
            Assert.Single(customer.DomainEvents);

        // Assert
        Assert.NotEqual(
            Guid.Empty,
            domainEvent.EventId);

        Assert.NotEqual(
            default,
            domainEvent.OccurredOnUtc);
    }

    private static Customer CreateCustomer()
    {
        return Customer.Create(
            firstName: "John",
            lastName: "Doe",
            email: Email.Create("john.doe@example.com"),
            phoneNumber: PhoneNumber.Create("+123", "4567890"),
            address: Address.Create(
                "123 Main Street",
                "Islamabad",
                "Punjab",
                "44000",
                "Pakistan"));
    }
}

public sealed class ProductDomainEventTests
{
    [Fact]
    public void ProductCreated_ShouldRaiseProductCreatedDomainEvent()
    {
        // Arrange
        var product = CreateProduct();

        // Act
        var domainEvent =
            Assert.Single(product.DomainEvents);

        // Assert
        var createdEvent =
            Assert.IsType<ProductCreatedDomainEvent>(
                domainEvent);

        Assert.Equal(
            product.Id,
            createdEvent.ProductId);
    }

    [Fact]
    public void ProductChangePrice_ShouldRaisePriceChangedDomainEvent()
    {
        // Arrange
        var product = CreateProduct();

        product.ClearDomainEvents();

        var oldPrice = product.Price;
        var newPrice = Money.Create(
            1500m,
            "USD");

        // Act
        product.ChangePrice(newPrice);

        // Assert
        var domainEvent =
            Assert.Single(product.DomainEvents);

        var priceChangedEvent =
            Assert.IsType<ProductPriceChangedDomainEvent>(
                domainEvent);

        Assert.Equal(
            product.Id,
            priceChangedEvent.ProductId);

        Assert.Equal(
            oldPrice,
            priceChangedEvent.OldPrice);

        Assert.Equal(
            newPrice,
            priceChangedEvent.NewPrice);
    }

    [Fact]
    public void ProductActivate_ShouldRaiseProductActivatedDomainEvent()
    {
        // Arrange
        var product = CreateProduct();

        product.Deactivate();

        product.ClearDomainEvents();

        // Act
        product.Activate();

        // Assert
        var domainEvent =
            Assert.Single(product.DomainEvents);

        var activatedEvent =
            Assert.IsType<ProductActivatedDomainEvent>(
                domainEvent);

        Assert.Equal(
            product.Id,
            activatedEvent.ProductId);
    }

    [Fact]
    public void ProductDeactivate_ShouldRaiseProductDeactivatedDomainEvent()
    {
        // Arrange
        var product = CreateProduct();

        product.ClearDomainEvents();

        // Act
        product.Deactivate();

        // Assert
        var domainEvent =
            Assert.Single(product.DomainEvents);

        var deactivatedEvent =
            Assert.IsType<ProductDeactivatedDomainEvent>(
                domainEvent);

        Assert.Equal(
            product.Id,
            deactivatedEvent.ProductId);
    }

    [Fact]
    public void ProductDiscontinue_ShouldNotRaiseDomainEvent()
    {
        // Arrange
        var product = CreateProduct();

        product.ClearDomainEvents();

        // Act
        product.Discontinue();

        // Assert
        Assert.Empty(product.DomainEvents);
    }

    [Fact]
    public void ProductPriceChangedEvent_ShouldContainOldAndNewPrices()
    {
        // Arrange
        var product = CreateProduct();

        product.ClearDomainEvents();

        var newPrice =
            Money.Create(2000m, "USD");

        // Act
        product.ChangePrice(newPrice);

        // Assert
        var priceChangedEvent =
            Assert.IsType<ProductPriceChangedDomainEvent>(
                Assert.Single(product.DomainEvents));

        Assert.Equal(
            1000m,
            priceChangedEvent.OldPrice.Amount);

        Assert.Equal(
            2000m,
            priceChangedEvent.NewPrice.Amount);

        Assert.Equal(
            "USD",
            priceChangedEvent.OldPrice.Currency);

        Assert.Equal(
            "USD",
            priceChangedEvent.NewPrice.Currency);
    }

    private static Product CreateProduct()
    {
        return Product.Create(
            sku: "SKU-001",
            name: "Laptop",
            description: "Business laptop",
            price: Money.Create(
                1000m,
                "USD"));
    }
}


public sealed class OrderDomainEventTests
{
    [Fact]
    public void OrderCreated_ShouldRaiseOrderCreatedDomainEvent()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var order =
            Order.Create(customerId);

        // Assert
        var domainEvent =
            Assert.Single(order.DomainEvents);

        var createdEvent =
            Assert.IsType<OrderCreatedDomainEvent>(
                domainEvent);

        Assert.Equal(
            order.Id,
            createdEvent.OrderId);

        Assert.Equal(
            customerId,
            createdEvent.CustomerId);

        Assert.Equal(
            0m,
            createdEvent.TotalAmount);
    }

    [Fact]
    public void OrderConfirm_ShouldRaiseOrderConfirmedDomainEvent()
    {
        // Arrange
        var order = CreateOrderWithItem();

        order.ClearDomainEvents();

        // Act
        order.Confirm();

        // Assert
        var domainEvent =
            Assert.Single(order.DomainEvents);

        var confirmedEvent =
            Assert.IsType<OrderConfirmedDomainEvent>(
                domainEvent);

        Assert.Equal(
            order.Id,
            confirmedEvent.OrderId);
    }

    [Fact]
    public void OrderCancel_ShouldRaiseOrderCancelledDomainEvent()
    {
        // Arrange
        var order = CreateOrderWithItem();

        order.ClearDomainEvents();

        // Act
        order.Cancel("Customer requested cancellation.");

        // Assert
        var domainEvent =
            Assert.Single(order.DomainEvents);

        var cancelledEvent =
            Assert.IsType<OrderCancelledDomainEvent>(
                domainEvent);

        Assert.Equal(
            order.Id,
            cancelledEvent.OrderId);

        Assert.Equal(
            "Customer requested cancellation.",
            cancelledEvent.Reason);
    }

    [Fact]
    public void OrderShip_ShouldRaiseOrderShippedDomainEvent()
    {
        // Arrange
        var order = CreateConfirmedOrder();

        order.ClearDomainEvents();

        // Act
        order.Ship();

        // Assert
        var domainEvent =
            Assert.Single(order.DomainEvents);

        var shippedEvent =
            Assert.IsType<OrderShippedDomainEvent>(
                domainEvent);

        Assert.Equal(
            order.Id,
            shippedEvent.OrderId);
    }

    [Fact]
    public void OrderDeliver_ShouldRaiseOrderDeliveredDomainEvent()
    {
        // Arrange
        var order = CreateConfirmedOrder();

        order.Ship();

        order.ClearDomainEvents();

        // Act
        order.Deliver();

        // Assert
        var domainEvent =
            Assert.Single(order.DomainEvents);

        var deliveredEvent =
            Assert.IsType<OrderDeliveredDomainEvent>(
                domainEvent);

        Assert.Equal(
            order.Id,
            deliveredEvent.OrderId);
    }

    [Fact]
    public void OrderDomainEvents_ShouldHaveValidMetadata()
    {
        // Arrange
        var order = CreateOrderWithItem();

        // Assert
        foreach (var domainEvent in order.DomainEvents)
        {
            Assert.NotEqual(
                Guid.Empty,
                domainEvent.EventId);

            Assert.NotEqual(
                default,
                domainEvent.OccurredOnUtc);
        }
    }

    private static Order CreateOrderWithItem()
    {
        var order =
            Order.Create(Guid.NewGuid());

        order.AddItem(
            productId: Guid.NewGuid(),
            productName: "Laptop",
            unitPrice: Money.Create(
                1000m,
                "USD"),
            quantity: 2);

        return order;
    }

    private static Order CreateConfirmedOrder()
    {
        var order =
            CreateOrderWithItem();

        order.Confirm();

        return order;
    }
}


public sealed class InventoryDomainEventTests
{
    [Fact]
    public void InventoryCreate_ShouldRaiseInventoryCreatedDomainEvent()
    {
        // Arrange
        var inventoryId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();

        // Act
        var inventory =
            Inventory.Create(
                inventoryId,
                productId,
                warehouseId,
                reorderLevel: 5,
                maximumQuantity: 100);

        // Assert
        var domainEvent =
            Assert.Single(inventory.DomainEvents);

        var createdEvent =
            Assert.IsType<InventoryCreatedDomainEvent>(
                domainEvent);

        Assert.Equal(
            inventoryId,
            createdEvent.InventoryId);

        Assert.Equal(
            productId,
            createdEvent.ProductId);

        Assert.Equal(
            warehouseId,
            createdEvent.WarehouseId);
    }

    [Fact]
    public void ReceiveStock_ShouldRaiseStockReceivedDomainEvent()
    {
        // Arrange
        var inventory = CreateInventory();

        inventory.ClearDomainEvents();

        // Act
        inventory.ReceiveStock(
            quantity: 10,
            reference: "PO-001",
            reason: "Initial stock");

        // Assert
        var domainEvent =
            Assert.Single(inventory.DomainEvents);

        var receivedEvent =
            Assert.IsType<StockReceivedDomainEvent>(
                domainEvent);

        Assert.Equal(
            inventory.Id,
            receivedEvent.InventoryId);

        Assert.Equal(
            inventory.ProductId,
            receivedEvent.ProductId);

        Assert.Equal(
            inventory.WarehouseId,
            receivedEvent.WarehouseId);

        Assert.Equal(
            10,
            receivedEvent.Quantity);

        Assert.Equal(
            "PO-001",
            receivedEvent.Reference);

        Assert.Equal(
            "Initial stock",
            receivedEvent.Reason);
    }

    [Fact]
    public void ReserveStock_ShouldRaiseStockReservedDomainEvent()
    {
        // Arrange
        var inventory = CreateInventory();

        inventory.ReceiveStock(
            10,
            "PO-001",
            "Initial stock");

        inventory.ClearDomainEvents();

        // Act
        inventory.ReserveStock(
            3,
            "ORDER-001",
            "Order reservation");

        // Assert
        var domainEvent =
            Assert.Single(inventory.DomainEvents);

        var reservedEvent =
            Assert.IsType<StockReservedDomainEvent>(
                domainEvent);

        Assert.Equal(
            inventory.Id,
            reservedEvent.InventoryId);

        Assert.Equal(
            3,
            reservedEvent.Quantity);

        Assert.Equal(
            "ORDER-001",
            reservedEvent.Reference);

        Assert.Equal(
            "Order reservation",
            reservedEvent.Reason);
    }

    [Fact]
    public void ReleaseReservedStock_ShouldRaiseStockReleasedDomainEvent()
    {
        // Arrange
        var inventory = CreateInventory();

        inventory.ReceiveStock(
            10,
            "PO-001",
            "Initial stock");

        inventory.ReserveStock(
            3,
            "ORDER-001",
            "Order reservation");

        inventory.ClearDomainEvents();

        // Act
        inventory.ReleaseReservedStock(
            2,
            "ORDER-001",
            "Order cancellation");

        // Assert
        var domainEvent =
            Assert.Single(inventory.DomainEvents);

        var releasedEvent =
            Assert.IsType<StockReleasedDomainEvent>(
                domainEvent);

        Assert.Equal(
            inventory.Id,
            releasedEvent.InventoryId);

        Assert.Equal(
            2,
            releasedEvent.Quantity);

        Assert.Equal(
            "ORDER-001",
            releasedEvent.Reference);

        Assert.Equal(
            "Order cancellation",
            releasedEvent.Reason);
    }

    [Fact]
    public void CommitReservation_ShouldRaiseStockCommittedDomainEvent()
    {
        // Arrange
        var inventory = CreateInventory();

        inventory.ReceiveStock(
            10,
            "PO-001",
            "Initial stock");

        inventory.ReserveStock(
            3,
            "ORDER-001",
            "Order reservation");

        inventory.ClearDomainEvents();

        // Act
        inventory.CommitReservation(
            3,
            "ORDER-001",
            "Order shipped");

        // Assert
        var domainEvent =
            Assert.Single(inventory.DomainEvents);

        var committedEvent =
            Assert.IsType<StockCommittedDomainEvent>(
                domainEvent);

        Assert.Equal(
            inventory.Id,
            committedEvent.InventoryId);

        Assert.Equal(
            3,
            committedEvent.Quantity);

        Assert.Equal(
            "ORDER-001",
            committedEvent.Reference);

        Assert.Equal(
            "Order shipped",
            committedEvent.Reason);
    }

    [Fact]
    public void AdjustStock_ShouldRaiseStockAdjustedDomainEvent()
    {
        // Arrange
        var inventory = CreateInventory();

        inventory.ReceiveStock(
            10,
            "PO-001",
            "Initial stock");

        inventory.ClearDomainEvents();

        // Act
        inventory.AdjustStock(
            newAvailableQuantity: 7,
            reference: "ADJ-001",
            reason: "Stock recount");

        // Assert
        var domainEvent =
            Assert.Single(inventory.DomainEvents);

        var adjustedEvent =
            Assert.IsType<StockAdjustedDomainEvent>(
                domainEvent);

        Assert.Equal(
            inventory.Id,
            adjustedEvent.InventoryId);

        Assert.Equal(
            10,
            adjustedEvent.PreviousAvailableQuantity);

        Assert.Equal(
            7,
            adjustedEvent.CurrentAvailableQuantity);

        Assert.Equal(
            "ADJ-001",
            adjustedEvent.Reference);

        Assert.Equal(
            "Stock recount",
            adjustedEvent.Reason);
    }

    [Fact]
    public void Discontinue_ShouldRaiseInventoryDiscontinuedDomainEvent()
    {
        // Arrange
        var inventory = CreateInventory();

        inventory.ClearDomainEvents();

        // Act
        inventory.Discontinue(
            "Product discontinued.");

        // Assert
        var domainEvent =
            Assert.Single(inventory.DomainEvents);

        var discontinuedEvent =
            Assert.IsType<InventoryDiscontinuedDomainEvent>(
                domainEvent);

        Assert.Equal(
            inventory.Id,
            discontinuedEvent.InventoryId);

        Assert.Equal(
            inventory.ProductId,
            discontinuedEvent.ProductId);

        Assert.Equal(
            inventory.WarehouseId,
            discontinuedEvent.WarehouseId);

        Assert.Equal(
            "Product discontinued.",
            discontinuedEvent.Reason);
    }

    [Fact]
    public void Reactivate_ShouldRaiseInventoryReactivatedDomainEvent()
    {
        // Arrange
        var inventory = CreateInventory();

        inventory.Discontinue(
            "Temporarily discontinued.");

        inventory.ClearDomainEvents();

        // Act
        inventory.Reactivate();

        // Assert
        var reactivatedEvent =
            inventory.DomainEvents
                .OfType<InventoryReactivatedDomainEvent>()
                .Single();

        Assert.Equal(
            inventory.Id,
            reactivatedEvent.InventoryId);

        Assert.Equal(
            inventory.ProductId,
            reactivatedEvent.ProductId);

        Assert.Equal(
            inventory.WarehouseId,
            reactivatedEvent.WarehouseId);
    }

    [Fact]
    public void LowStock_ShouldRaiseLowStockDetectedDomainEvent()
    {
        // Arrange
        var inventory = CreateInventory(
            reorderLevel: 5);

        inventory.ReceiveStock(
            10,
            "PO-001",
            "Initial stock");

        inventory.ClearDomainEvents();

        // Act
        inventory.AdjustStock(
            newAvailableQuantity: 5,
            reference: "ADJ-001",
            reason: "Stock adjustment");

        // Assert
        Assert.Contains(
            inventory.DomainEvents,
            x => x is LowStockDetectedDomainEvent);

        var lowStockEvent =
            inventory.DomainEvents
                .OfType<LowStockDetectedDomainEvent>()
                .Single();

        Assert.Equal(
            inventory.Id,
            lowStockEvent.InventoryId);

        Assert.Equal(
            inventory.ProductId,
            lowStockEvent.ProductId);

        Assert.Equal(
            inventory.WarehouseId,
            lowStockEvent.WarehouseId);

        Assert.Equal(
            5,
            lowStockEvent.AvailableQuantity);

        Assert.Equal(
            5,
            lowStockEvent.ReorderLevel);
    }

    [Fact]
    public void OutOfStock_ShouldRaiseOutOfStockDomainEvent()
    {
        // Arrange
        var inventory = CreateInventory();

        inventory.ReceiveStock(
            10,
            "PO-001",
            "Initial stock");

        inventory.ClearDomainEvents();

        // Act
        inventory.AdjustStock(
            newAvailableQuantity: 0,
            reference: "ADJ-001",
            reason: "Stock depletion");

        // Assert
        Assert.Contains(
            inventory.DomainEvents,
            x => x is OutOfStockDomainEvent);

        var outOfStockEvent =
            inventory.DomainEvents
                .OfType<OutOfStockDomainEvent>()
                .Single();

        Assert.Equal(
            inventory.Id,
            outOfStockEvent.InventoryId);

        Assert.Equal(
            inventory.ProductId,
            outOfStockEvent.ProductId);

        Assert.Equal(
            inventory.WarehouseId,
            outOfStockEvent.WarehouseId);
    }

    [Fact]
    public void InventoryDomainEvents_ShouldHaveValidMetadata()
    {
        // Arrange
        var inventory = CreateInventory();

        // Assert
        foreach (var domainEvent in inventory.DomainEvents)
        {
            Assert.NotEqual(
                Guid.Empty,
                domainEvent.EventId);

            Assert.NotEqual(
                default,
                domainEvent.OccurredOnUtc);
        }
    }

    private static Inventory CreateInventory(
        int reorderLevel = 2)
    {
        return Inventory.Create(
            inventoryId: Guid.NewGuid(),
            productId: Guid.NewGuid(),
            warehouseId: Guid.NewGuid(),
            reorderLevel: reorderLevel,
            maximumQuantity: 100);
    }
}


public sealed class PaymentDomainEventTests
{
    [Fact]
    public void PaymentCreate_ShouldRaisePaymentCreatedDomainEvent()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        // Act
        var payment =
            Payment.Create(
                paymentId,
                orderId,
                1000m,
                "USD",
                PaymentMethod.CreditCard,
                PaymentProvider.Stripe);

        // Assert
        var domainEvent =
            Assert.Single(payment.DomainEvents);

        var createdEvent =
            Assert.IsType<PaymentCreatedDomainEvent>(
                domainEvent);

        Assert.Equal(
            paymentId,
            createdEvent.PaymentId);

        Assert.Equal(
            orderId,
            createdEvent.OrderId);

        Assert.Equal(
            1000m,
            createdEvent.Amount);

        Assert.Equal(
            "USD",
            createdEvent.Currency);
    }

    [Fact]
    public void Authorize_ShouldRaisePaymentAuthorizedDomainEvent()
    {
        // Arrange
        var payment = CreatePayment();

        payment.ClearDomainEvents();

        // Act
        payment.Authorize(
            "AUTH-001",
            "AUTH-CODE-001");

        // Assert
        var domainEvent =
            Assert.Single(payment.DomainEvents);

        var authorizedEvent =
            Assert.IsType<PaymentAuthorizedDomainEvent>(
                domainEvent);

        Assert.Equal(
            payment.Id,
            authorizedEvent.PaymentId);

        Assert.Equal(
            payment.OrderId,
            authorizedEvent.OrderId);

        Assert.Equal(
            payment.Amount,
            authorizedEvent.Amount);

        Assert.Equal(
            "AUTH-001",
            authorizedEvent.ProviderReference);
    }

    [Fact]
    public void Capture_ShouldRaisePaymentCapturedDomainEvent()
    {
        // Arrange
        var payment = CreateAuthorizedPayment();

        payment.ClearDomainEvents();

        // Act
        payment.Capture(
            "CAPTURE-001");

        // Assert
        var domainEvent =
            Assert.Single(payment.DomainEvents);

        var capturedEvent =
            Assert.IsType<PaymentCapturedDomainEvent>(
                domainEvent);

        Assert.Equal(
            payment.Id,
            capturedEvent.PaymentId);

        Assert.Equal(
            payment.OrderId,
            capturedEvent.OrderId);

        Assert.Equal(
            payment.Amount,
            capturedEvent.Amount);

        Assert.Equal(
            "CAPTURE-001",
            capturedEvent.ProviderReference);
    }

    [Fact]
    public void Settle_ShouldRaisePaymentSettledDomainEvent()
    {
        // Arrange
        var payment = CreateCapturedPayment();

        payment.ClearDomainEvents();

        // Act
        payment.Settle(
            "SETTLEMENT-001");

        // Assert
        var domainEvent =
            Assert.Single(payment.DomainEvents);

        var settledEvent =
            Assert.IsType<PaymentSettledDomainEvent>(
                domainEvent);

        Assert.Equal(
            payment.Id,
            settledEvent.PaymentId);

        Assert.Equal(
            payment.OrderId,
            settledEvent.OrderId);

        Assert.Equal(
            payment.Amount,
            settledEvent.Amount);

        Assert.Equal(
            "SETTLEMENT-001",
            settledEvent.ProviderReference);
    }

    [Fact]
    public void Refund_ShouldRaisePaymentRefundedDomainEvent()
    {
        // Arrange
        var payment = CreateCapturedPayment();

        payment.ClearDomainEvents();

        // Act
        payment.Refund(
            refundAmount: 250m,
            refundReference: "REFUND-001",
            reason: "Customer refund");

        // Assert
        var domainEvent =
            Assert.Single(payment.DomainEvents);

        var refundedEvent =
            Assert.IsType<PaymentRefundedDomainEvent>(
                domainEvent);

        Assert.Equal(
            payment.Id,
            refundedEvent.PaymentId);

        Assert.Equal(
            payment.OrderId,
            refundedEvent.OrderId);

        Assert.Equal(
            250m,
            refundedEvent.RefundAmount);

        Assert.Equal(
            250m,
            refundedEvent.TotalRefundedAmount);

        Assert.Equal(
            "REFUND-001",
            refundedEvent.RefundReference);

        Assert.Equal(
            "Customer refund",
            refundedEvent.Reason);
    }

    [Fact]
    public void MultipleRefunds_ShouldRaiseEventWithCumulativeTotal()
    {
        // Arrange
        var payment = CreateCapturedPayment();

        payment.ClearDomainEvents();

        payment.Refund(
            250m,
            "REFUND-001",
            "Partial refund");

        payment.ClearDomainEvents();

        // Act
        payment.Refund(
            300m,
            "REFUND-002",
            "Second refund");

        // Assert
        var refundedEvent =
            Assert.IsType<PaymentRefundedDomainEvent>(
                Assert.Single(payment.DomainEvents));

        Assert.Equal(
            300m,
            refundedEvent.RefundAmount);

        Assert.Equal(
            550m,
            refundedEvent.TotalRefundedAmount);

        Assert.Equal(
            "REFUND-002",
            refundedEvent.RefundReference);

        Assert.Equal(
            "Second refund",
            refundedEvent.Reason);
    }

    [Fact]
    public void Cancel_ShouldRaisePaymentCancelledDomainEvent()
    {
        // Arrange
        var payment = CreatePayment();

        payment.ClearDomainEvents();

        // Act
        payment.Cancel(
            "Customer cancelled payment.");

        // Assert
        var domainEvent =
            Assert.Single(payment.DomainEvents);

        var cancelledEvent =
            Assert.IsType<PaymentCancelledDomainEvent>(
                domainEvent);

        Assert.Equal(
            payment.Id,
            cancelledEvent.PaymentId);

        Assert.Equal(
            payment.OrderId,
            cancelledEvent.OrderId);

        Assert.Equal(
            "Customer cancelled payment.",
            cancelledEvent.Reason);
    }

    [Fact]
    public void Fail_ShouldRaisePaymentFailedDomainEvent()
    {
        // Arrange
        var payment = CreatePayment();

        payment.ClearDomainEvents();

        // Act
        payment.Fail(
            failureCode: "DECLINED",
            failureReason: "Payment was declined.");

        // Assert
        var domainEvent =
            Assert.Single(payment.DomainEvents);

        var failedEvent =
            Assert.IsType<PaymentFailedDomainEvent>(
                domainEvent);

        Assert.Equal(
            payment.Id,
            failedEvent.PaymentId);

        Assert.Equal(
            payment.OrderId,
            failedEvent.OrderId);

        Assert.Equal(
            payment.Amount,
            failedEvent.Amount);

        Assert.Equal(
            "DECLINED",
            failedEvent.FailureCode);

        Assert.Equal(
            "Payment was declined.",
            failedEvent.FailureReason);
    }

    [Fact]
    public void PaymentDomainEvents_ShouldHaveValidMetadata()
    {
        // Arrange
        var payment = CreatePayment();

        // Assert
        foreach (var domainEvent in payment.DomainEvents)
        {
            Assert.NotEqual(
                Guid.Empty,
                domainEvent.EventId);

            Assert.NotEqual(
                default,
                domainEvent.OccurredOnUtc);
        }
    }

    private static Payment CreatePayment()
    {
        return Payment.Create(
            paymentId: Guid.NewGuid(),
            orderId: Guid.NewGuid(),
            amount: 1000m,
            currency: "USD",
            paymentMethod: PaymentMethod.CreditCard,
            paymentProvider: PaymentProvider.Stripe);
    }

    private static Payment CreateAuthorizedPayment()
    {
        var payment = CreatePayment();

        payment.Authorize(
            "AUTH-001",
            "AUTH-CODE-001");

        payment.ClearDomainEvents();

        return payment;
    }

    private static Payment CreateCapturedPayment()
    {
        var payment =
            CreateAuthorizedPayment();

        payment.Capture(
            "CAPTURE-001");

        payment.ClearDomainEvents();

        return payment;
    }
}
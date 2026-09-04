using EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.PaymentAggregate;
using EnterpriseOrderManagementSystem.Domain.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Common.ValueObjects;
using EnterpriseOrderManagementSystem.Domain.Enums;
using EnterpriseOrderManagementSystem.Domain.Specifications;
using EnterpriseOrderManagementSystem.Domain.Specifications.InventorySpecifications;
using EnterpriseOrderManagementSystem.Domain.Specifications.OrderSpecifications;
using EnterpriseOrderManagementSystem.Domain.Specifications.PaymentSpecifications;
using Xunit;

namespace EnterpriseOrderManagementSystem.Domain.Tests.Specifications;

public sealed class SpecificationCompositionTests
{
    [Fact]
    public void And_ShouldReturnTrue_WhenBothSpecificationsAreSatisfied()
    {
        // Arrange
        var left = new AlwaysTrueSpecification();
        var right = new AlwaysTrueSpecification();

        var specification = left.And(right);

        // Act
        var result = specification.IsSatisfiedBy(new object());

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void And_ShouldReturnFalse_WhenLeftSpecificationIsNotSatisfied()
    {
        // Arrange
        var left = new AlwaysFalseSpecification();
        var right = new AlwaysTrueSpecification();

        var specification = left.And(right);

        // Act
        var result = specification.IsSatisfiedBy(new object());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void And_ShouldReturnFalse_WhenRightSpecificationIsNotSatisfied()
    {
        // Arrange
        var left = new AlwaysTrueSpecification();
        var right = new AlwaysFalseSpecification();

        var specification = left.And(right);

        // Act
        var result = specification.IsSatisfiedBy(new object());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Or_ShouldReturnTrue_WhenLeftSpecificationIsSatisfied()
    {
        // Arrange
        var left = new AlwaysTrueSpecification();
        var right = new AlwaysFalseSpecification();

        var specification = left.Or(right);

        // Act
        var result = specification.IsSatisfiedBy(new object());

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Or_ShouldReturnTrue_WhenRightSpecificationIsSatisfied()
    {
        // Arrange
        var left = new AlwaysFalseSpecification();
        var right = new AlwaysTrueSpecification();

        var specification = left.Or(right);

        // Act
        var result = specification.IsSatisfiedBy(new object());

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Or_ShouldReturnFalse_WhenBothSpecificationsAreNotSatisfied()
    {
        // Arrange
        var left = new AlwaysFalseSpecification();
        var right = new AlwaysFalseSpecification();

        var specification = left.Or(right);

        // Act
        var result = specification.IsSatisfiedBy(new object());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Not_ShouldReturnTrue_WhenWrappedSpecificationIsNotSatisfied()
    {
        // Arrange
        var specification =
            new AlwaysFalseSpecification()
                .Not();

        // Act
        var result = specification.IsSatisfiedBy(new object());

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Not_ShouldReturnFalse_WhenWrappedSpecificationIsSatisfied()
    {
        // Arrange
        var specification =
            new AlwaysTrueSpecification()
                .Not();

        // Act
        var result = specification.IsSatisfiedBy(new object());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void And_ShouldThrow_WhenSpecificationIsNull()
    {
        // Arrange
        var specification = new AlwaysTrueSpecification();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            specification.And(null!));
    }

    [Fact]
    public void Or_ShouldThrow_WhenSpecificationIsNull()
    {
        // Arrange
        var specification = new AlwaysTrueSpecification();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            specification.Or(null!));
    }

    [Fact]
    public void AndSpecification_ShouldThrow_WhenLeftIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new AndSpecification<object>(
                null!,
                new AlwaysTrueSpecification()));
    }

    [Fact]
    public void AndSpecification_ShouldThrow_WhenRightIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new AndSpecification<object>(
                new AlwaysTrueSpecification(),
                null!));
    }

    [Fact]
    public void OrSpecification_ShouldThrow_WhenLeftIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new OrSpecification<object>(
                null!,
                new AlwaysTrueSpecification()));
    }

    [Fact]
    public void OrSpecification_ShouldThrow_WhenRightIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new OrSpecification<object>(
                new AlwaysTrueSpecification(),
                null!));
    }

    [Fact]
    public void NotSpecification_ShouldThrow_WhenWrappedSpecificationIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new NotSpecification<object>(
                null!));
    }

    private sealed class AlwaysTrueSpecification
        : Specification<object>
    {
        public override bool IsSatisfiedBy(object entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return true;
        }
    }

    private sealed class AlwaysFalseSpecification
        : Specification<object>
    {
        public override bool IsSatisfiedBy(object entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return false;
        }
    }
}

public sealed class InventorySpecificationTests
{
    [Fact]
    public void InventoryHasAvailableStock_ShouldReturnTrue_WhenStockIsAvailable()
    {
        // Arrange
        var inventory = CreateInventory();

        inventory.ReceiveStock(
            quantity: 10,
            reference: "PO-001",
            reason: "Initial stock");

        var specification =
            new InventoryHasAvailableStockSpecification();

        // Act
        var result =
            specification.IsSatisfiedBy(inventory);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void InventoryHasAvailableStock_ShouldReturnFalse_WhenStockIsZero()
    {
        // Arrange
        var inventory = CreateInventory();

        var specification =
            new InventoryHasAvailableStockSpecification();

        // Act
        var result =
            specification.IsSatisfiedBy(inventory);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void InventoryHasAvailableStock_ShouldThrow_WhenInventoryIsNull()
    {
        // Arrange
        var specification =
            new InventoryHasAvailableStockSpecification();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            specification.IsSatisfiedBy(null!));
    }

    [Fact]
    public void InventoryCanReserve_ShouldReturnTrue_WhenAvailableQuantityIsEnough()
    {
        // Arrange
        var inventory = CreateInventory();

        inventory.ReceiveStock(
            quantity: 10,
            reference: "PO-001",
            reason: "Initial stock");

        var specification =
            new InventoryCanReserveSpecification(5);

        // Act
        var result =
            specification.IsSatisfiedBy(inventory);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void InventoryCanReserve_ShouldReturnTrue_WhenRequestedQuantityEqualsAvailableQuantity()
    {
        // Arrange
        var inventory = CreateInventory();

        inventory.ReceiveStock(
            quantity: 10,
            reference: "PO-001",
            reason: "Initial stock");

        var specification =
            new InventoryCanReserveSpecification(10);

        // Act
        var result =
            specification.IsSatisfiedBy(inventory);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void InventoryCanReserve_ShouldReturnFalse_WhenAvailableQuantityIsInsufficient()
    {
        // Arrange
        var inventory = CreateInventory();

        inventory.ReceiveStock(
            quantity: 5,
            reference: "PO-001",
            reason: "Initial stock");

        var specification =
            new InventoryCanReserveSpecification(10);

        // Act
        var result =
            specification.IsSatisfiedBy(inventory);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void InventoryCanReserve_ShouldThrow_WhenQuantityIsZero()
    {
        // Act & Assert
        var exception =
            Assert.Throws<BusinessRuleViolationException>(() =>
                new InventoryCanReserveSpecification(0));

        Assert.Contains(
            "greater than zero",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void InventoryCanReserve_ShouldThrow_WhenQuantityIsNegative()
    {
        // Act & Assert
        var exception =
            Assert.Throws<BusinessRuleViolationException>(() =>
                new InventoryCanReserveSpecification(-1));

        Assert.Contains(
            "greater than zero",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void InventoryCanReserve_ShouldThrow_WhenInventoryIsNull()
    {
        // Arrange
        var specification =
            new InventoryCanReserveSpecification(5);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            specification.IsSatisfiedBy(null!));
    }

    private static Inventory CreateInventory()
    {
        return Inventory.Create(
            inventoryId: Guid.NewGuid(),
            productId: Guid.NewGuid(),
            warehouseId: Guid.NewGuid(),
            reorderLevel: 2,
            maximumQuantity: 100);
    }
}

public sealed class OrderSpecificationTests
{
    [Fact]
    public void ConfirmedOrderSpecification_ShouldReturnFalse_WhenOrderIsPending()
    {
        // Arrange
        var order =
            Order.Create(Guid.NewGuid());

        var specification =
            new ConfirmedOrderSpecification();

        // Act
        var result =
            specification.IsSatisfiedBy(order);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ConfirmedOrderSpecification_ShouldReturnTrue_WhenOrderIsConfirmed()
    {
        // Arrange
        var order =
            Order.Create(Guid.NewGuid());

        order.AddItem(
            productId: Guid.NewGuid(),
            productName: "Laptop",
            unitPrice: Money.Create(1000m, "USD"),
            quantity: 1);

        order.Confirm();

        var specification =
            new ConfirmedOrderSpecification();

        // Act
        var result =
            specification.IsSatisfiedBy(order);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ConfirmedOrderSpecification_ShouldThrow_WhenOrderIsNull()
    {
        // Arrange
        var specification =
            new ConfirmedOrderSpecification();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            specification.IsSatisfiedBy(null!));
    }

    [Fact]
    public void OrderHasItemsSpecification_ShouldReturnFalse_WhenOrderHasNoItems()
    {
        // Arrange
        var order =
            Order.Create(Guid.NewGuid());

        var specification =
            new OrderHasItemsSpecification();

        // Act
        var result =
            specification.IsSatisfiedBy(order);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void OrderHasItemsSpecification_ShouldReturnTrue_WhenOrderHasItems()
    {
        // Arrange
        var order =
            Order.Create(Guid.NewGuid());

        order.AddItem(
            productId: Guid.NewGuid(),
            productName: "Laptop",
            unitPrice: Money.Create(1000m, "USD"),
            quantity: 1);

        var specification =
            new OrderHasItemsSpecification();

        // Act
        var result =
            specification.IsSatisfiedBy(order);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OrderHasItemsSpecification_ShouldThrow_WhenOrderIsNull()
    {
        // Arrange
        var specification =
            new OrderHasItemsSpecification();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            specification.IsSatisfiedBy(null!));
    }
}

public sealed class PaymentSpecificationTests
{
    [Fact]
    public void PaymentCapturedOrSettled_ShouldReturnFalse_WhenPaymentIsPending()
    {
        // Arrange
        var payment =
            CreatePayment();

        var specification =
            new PaymentCapturedOrSettledSpecification();

        // Act
        var result =
            specification.IsSatisfiedBy(payment);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void PaymentCapturedOrSettled_ShouldReturnTrue_WhenPaymentIsCaptured()
    {
        // Arrange
        var payment =
            CreatePayment();

        payment.Authorize(
            providerReference: "AUTH-001",
            authorizationCode: "AUTH-CODE-001");

        payment.Capture(
            providerReference: "CAPTURE-001");

        var specification =
            new PaymentCapturedOrSettledSpecification();

        // Act
        var result =
            specification.IsSatisfiedBy(payment);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void PaymentCapturedOrSettled_ShouldReturnTrue_WhenPaymentIsSettled()
    {
        // Arrange
        var payment =
            CreatePayment();

        payment.Authorize(
            providerReference: "AUTH-001",
            authorizationCode: "AUTH-CODE-001");

        payment.Capture(
            providerReference: "CAPTURE-001");

        payment.Settle(
            providerReference: "SETTLEMENT-001");

        var specification =
            new PaymentCapturedOrSettledSpecification();

        // Act
        var result =
            specification.IsSatisfiedBy(payment);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void PaymentCapturedOrSettled_ShouldThrow_WhenPaymentIsNull()
    {
        // Arrange
        var specification =
            new PaymentCapturedOrSettledSpecification();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            specification.IsSatisfiedBy(null!));
    }

    [Fact]
    public void PaymentRefundable_ShouldReturnFalse_WhenPaymentIsPending()
    {
        // Arrange
        var payment =
            CreatePayment();

        var specification =
            new PaymentRefundableSpecification();

        // Act
        var result =
            specification.IsSatisfiedBy(payment);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void PaymentRefundable_ShouldReturnTrue_WhenPaymentIsCaptured()
    {
        // Arrange
        var payment =
            CreatePayment();

        payment.Authorize(
            providerReference: "AUTH-001",
            authorizationCode: "AUTH-CODE-001");

        payment.Capture(
            providerReference: "CAPTURE-001");

        var specification =
            new PaymentRefundableSpecification();

        // Act
        var result =
            specification.IsSatisfiedBy(payment);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void PaymentRefundable_ShouldReturnTrue_WhenPaymentIsSettled()
    {
        // Arrange
        var payment =
            CreatePayment();

        payment.Authorize(
            providerReference: "AUTH-001",
            authorizationCode: "AUTH-CODE-001");

        payment.Capture(
            providerReference: "CAPTURE-001");

        payment.Settle(
            providerReference: "SETTLEMENT-001");

        var specification =
            new PaymentRefundableSpecification();

        // Act
        var result =
            specification.IsSatisfiedBy(payment);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void PaymentRefundable_ShouldThrow_WhenPaymentIsNull()
    {
        // Arrange
        var specification =
            new PaymentRefundableSpecification();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            specification.IsSatisfiedBy(null!));
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
}
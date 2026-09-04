using EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate.Events;
using EnterpriseOrderManagementSystem.Domain.Common.Exceptions;
using FluentAssertions;

namespace EnterpriseOrderManagementSystem.Domain.Tests.Aggregates.InventoryAggregate;

public sealed class InventoryTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidData_CreatesInventory()
    {
        // Arrange
        var inventoryId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();

        // Act
        var inventory = Inventory.Create(
            inventoryId,
            productId,
            warehouseId,
            reorderLevel: 10,
            maximumQuantity: 100);

        // Assert
        inventory.Should().NotBeNull();

        inventory.Id
            .Should()
            .Be(inventoryId);

        inventory.ProductId
            .Should()
            .Be(productId);

        inventory.WarehouseId
            .Should()
            .Be(warehouseId);

        inventory.ReorderLevel
            .Should()
            .Be(10);

        inventory.MaximumQuantity
            .Should()
            .Be(100);
    }

    [Fact]
    public void Create_WithEmptyInventoryId_ThrowsBusinessRuleViolationException()
    {
        // Act
        var action = () =>
            Inventory.Create(
                Guid.Empty,
                Guid.NewGuid(),
                Guid.NewGuid(),
                10,
                100);

        // Assert
        action.Should()
            .Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Create_WithEmptyProductId_ThrowsBusinessRuleViolationException()
    {
        // Act
        var action = () =>
            Inventory.Create(
                Guid.NewGuid(),
                Guid.Empty,
                Guid.NewGuid(),
                10,
                100);

        // Assert
        action.Should()
            .Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Create_WithEmptyWarehouseId_ThrowsBusinessRuleViolationException()
    {
        // Act
        var action = () =>
            Inventory.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.Empty,
                10,
                100);

        // Assert
        action.Should()
            .Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Create_WithNegativeReorderLevel_ThrowsBusinessRuleViolationException()
    {
        // Act
        var action = () =>
            Inventory.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                -1,
                100);

        // Assert
        action.Should()
            .Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Create_WithZeroMaximumQuantity_ThrowsBusinessRuleViolationException()
    {
        // Act
        var action = () =>
            Inventory.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                10,
                0);

        // Assert
        action.Should()
            .Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Create_WithNegativeMaximumQuantity_ThrowsBusinessRuleViolationException()
    {
        // Act
        var action = () =>
            Inventory.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                10,
                -1);

        // Assert
        action.Should()
            .Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Create_WhenReorderLevelExceedsMaximumQuantity_ThrowsBusinessRuleViolationException()
    {
        // Act
        var action = () =>
            Inventory.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                101,
                100);

        // Assert
        action.Should()
            .Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Create_WhenReorderLevelEqualsMaximumQuantity_CreatesInventory()
    {
        // Act
        var inventory = Inventory.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            100,
            100);

        // Assert
        inventory.Should().NotBeNull();

        inventory.ReorderLevel
            .Should()
            .Be(100);

        inventory.MaximumQuantity
            .Should()
            .Be(100);
    }

    [Fact]
    public void Create_WithValidData_RaisesInventoryCreatedEvent()
    {
        // Act
        var inventory = CreateValidInventory();

        // Assert
        inventory.DomainEvents
            .Should()
            .ContainSingle(eventItem =>
                eventItem is InventoryCreatedDomainEvent);
    }

    #endregion

    #region Quantity Tests

    [Fact]
    public void ReceiveStock_WithValidQuantity_IncreasesAvailableQuantity()
    {
        // Arrange
        var inventory = CreateValidInventory();

        // Act
        inventory.ReceiveStock(
            quantity: 20,
            reference: "PO-1001",
            reason: "Purchase order receipt");

        // Assert
        inventory.AvailableQuantity
            .Should()
            .Be(20);
    }

    [Fact]
    public void ReceiveStock_WithZeroQuantity_ThrowsBusinessRuleViolationException()
    {
        // Arrange
        var inventory = CreateValidInventory();

        // Act
        var action = () =>
            inventory.ReceiveStock(
                0,
                "PO-1001",
                "Purchase order receipt");

        // Assert
        action.Should()
            .Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void ReceiveStock_WithNegativeQuantity_ThrowsBusinessRuleViolationException()
    {
        // Arrange
        var inventory = CreateValidInventory();

        // Act
        var action = () =>
            inventory.ReceiveStock(
                -10,
                "PO-1001",
                "Purchase order receipt");

        // Assert
        action.Should()
            .Throw<BusinessRuleViolationException>();
    }

    #endregion

    #region Reservation Tests

    [Fact]
    public void Reserve_WithSufficientAvailableQuantity_UpdatesQuantities()
    {
        // Arrange
        var inventory = CreateValidInventory();

        inventory.ReceiveStock(
            100,
            "PO-1001",
            "Initial stock");

        // Act
        inventory.ReserveStock(
            20,
            "ORDER-1001",
            "Customer order");

        // Assert
        inventory.AvailableQuantity
            .Should()
            .Be(80);

        inventory.ReservedQuantity
            .Should()
            .Be(20);
    }

    [Fact]
    public void Reserve_WithInsufficientAvailableQuantity_ThrowsBusinessRuleViolationException()
    {
        // Arrange
        var inventory = CreateValidInventory();

        inventory.ReceiveStock(
            10,
            "PO-1001",
            "Initial stock");

        // Act
        var action = () =>
            inventory.ReserveStock(
                20,
                "ORDER-1001",
                "Customer order");

        // Assert
        action.Should()
            .Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Reserve_WithZeroQuantity_ThrowsBusinessRuleViolationException()
    {
        // Arrange
        var inventory = CreateValidInventory();

        inventory.ReceiveStock(
            100,
            "PO-1001",
            "Initial stock");

        // Act
        var action = () =>
            inventory.ReserveStock(
                0,
                "ORDER-1001",
                "Customer order");

        // Assert
        action.Should()
            .Throw<BusinessRuleViolationException>();
    }

    #endregion

    #region Release Tests

    [Fact]
    public void Release_WithSufficientReservedQuantity_RestoresAvailableQuantity()
    {
        // Arrange
        var inventory = CreateValidInventory();

        inventory.ReceiveStock(
            100,
            "PO-1001",
            "Initial stock");

        inventory.ReserveStock(
            20,
            "ORDER-1001",
            "Customer order");

        // Act
        inventory.ReleaseReservedStock(
            10,
            "ORDER-1001",
            "Order cancelled");

        // Assert
        inventory.AvailableQuantity
            .Should()
            .Be(90);

        inventory.ReservedQuantity
            .Should()
            .Be(10);
    }

    [Fact]
    public void Release_WhenQuantityExceedsReservedQuantity_ThrowsBusinessRuleViolationException()
    {
        // Arrange
        var inventory = CreateValidInventory();

        inventory.ReceiveStock(
            100,
            "PO-1001",
            "Initial stock");

        inventory.ReserveStock(
            20,
            "ORDER-1001",
            "Customer order");

        // Act
        var action = () =>
            inventory.ReleaseReservedStock(
                30,
                "ORDER-1001",
                "Order cancelled");

        // Assert
        action.Should()
            .Throw<BusinessRuleViolationException>();
    }

    #endregion

    #region Transaction Tests

    [Fact]
    public void ReceiveStock_WithValidQuantity_CreatesInventoryTransaction()
    {
        // Arrange
        var inventory = CreateValidInventory();

        // Act
        inventory.ReceiveStock(
            50,
            "PO-1001",
            "Purchase order receipt");

        // Assert
        inventory.Transactions
            .Should()
            .ContainSingle();
    }

    [Fact]
    public void Reserve_WithValidQuantity_CreatesInventoryTransaction()
    {
        // Arrange
        var inventory = CreateValidInventory();

        inventory.ReceiveStock(
            100,
            "PO-1001",
            "Initial stock");

        // Act
        inventory.ReserveStock(
            20,
            "ORDER-1001",
            "Customer order");

        // Assert
        inventory.Transactions
            .Should()
            .HaveCount(2);
    }

    [Fact]
    public void Release_WithValidQuantity_CreatesInventoryTransaction()
    {
        // Arrange
        var inventory = CreateValidInventory();

        inventory.ReceiveStock(
            100,
            "PO-1001",
            "Initial stock");

        inventory.ReserveStock(
            20,
            "ORDER-1001",
            "Customer order");

        // Act
        inventory.ReleaseReservedStock(
            10,
            "ORDER-1001",
            "Order cancelled");

        // Assert
        inventory.Transactions
            .Should()
            .HaveCount(3);
    }

    #endregion

    #region Status Tests

    [Fact]
    public void Create_WithZeroAvailableQuantity_HasExpectedInitialStatus()
    {
        // Arrange
        var inventory = CreateValidInventory();

        // Assert
        inventory.AvailableQuantity
            .Should()
            .Be(0);
    }

    [Fact]
    public void ReceiveStock_WhenQuantityReachesMaximum_DoesNotExceedMaximumQuantity()
    {
        // Arrange
        var inventory = CreateValidInventory();

        // Act
        inventory.ReceiveStock(
            100,
            "PO-1001",
            "Initial stock");

        // Assert
        inventory.AvailableQuantity
            .Should()
            .Be(100);
    }

    #endregion

    #region Domain Event Tests

    [Fact]
    public void Create_RaisesInventoryCreatedEventWithCorrectInventoryId()
    {
        // Arrange
        var inventoryId = Guid.NewGuid();

        var inventory = Inventory.Create(
            inventoryId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            10,
            100);

        // Act
        var domainEvent = inventory.DomainEvents
            .OfType<InventoryCreatedDomainEvent>()
            .Single();

        // Assert
        domainEvent.InventoryId
            .Should()
            .Be(inventory.Id);
    }

    #endregion

    #region Test Helpers

    private static Inventory CreateValidInventory()
    {
        return Inventory.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            reorderLevel: 10,
            maximumQuantity: 100);
    }

    #endregion
}
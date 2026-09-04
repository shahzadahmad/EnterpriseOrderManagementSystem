using EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate;
using EnterpriseOrderManagementSystem.Domain.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Enums;
using FluentAssertions;

namespace EnterpriseOrderManagementSystem.Domain.Tests.Aggregates.InventoryAggregate;

public sealed class InventoryTransactionTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidData_CreatesTransaction()
    {
        // Arrange
        var movementType = InventoryMovementType.Receive;
        var quantity = 100;
        var availableQuantityBefore = 0;
        var availableQuantityAfter = 100;
        var reservedQuantityBefore = 0;
        var reservedQuantityAfter = 0;
        var reference = "PO-1001";
        var reason = "Initial stock receipt";

        // Act
        var transaction = InventoryTransaction.Create(
            movementType,
            quantity,
            availableQuantityBefore,
            availableQuantityAfter,
            reservedQuantityBefore,
            reservedQuantityAfter,
            reference,
            reason);

        // Assert
        transaction.Should().NotBeNull();

        transaction.MovementType
            .Should()
            .Be(movementType);

        transaction.Quantity
            .Should()
            .Be(quantity);

        transaction.AvailableQuantityBefore
            .Should()
            .Be(availableQuantityBefore);

        transaction.AvailableQuantityAfter
            .Should()
            .Be(availableQuantityAfter);

        transaction.ReservedQuantityBefore
            .Should()
            .Be(reservedQuantityBefore);

        transaction.ReservedQuantityAfter
            .Should()
            .Be(reservedQuantityAfter);

        transaction.Reference
            .Should()
            .Be(reference);

        transaction.Reason
            .Should()
            .Be(reason);
    }

    [Fact]
    public void Create_WithValidData_GeneratesTransactionId()
    {
        // Act
        var transaction = CreateValidTransaction();

        // Assert
        transaction.Id
            .Should()
            .NotBe(Guid.Empty);
    }

    [Fact]
    public void Create_WithValidData_GeneratesUniqueTransactionIds()
    {
        // Act
        var transaction1 = CreateValidTransaction();
        var transaction2 = CreateValidTransaction();

        // Assert
        transaction1.Id
            .Should()
            .NotBe(Guid.Empty);

        transaction2.Id
            .Should()
            .NotBe(Guid.Empty);

        transaction1.Id
            .Should()
            .NotBe(transaction2.Id);
    }

    [Fact]
    public void Create_WithValidData_SetsOccurredOnUtc()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var transaction = CreateValidTransaction();

        var afterCreation = DateTime.UtcNow;

        // Assert
        transaction.OccurredOnUtc
            .Should()
            .BeOnOrAfter(beforeCreation);

        transaction.OccurredOnUtc
            .Should()
            .BeOnOrBefore(afterCreation);
    }

    #endregion

    #region Quantity Validation Tests

    [Fact]
    public void Create_WithZeroQuantity_ThrowsBusinessRuleViolationException()
    {
        // Act
        var action = () =>
            InventoryTransaction.Create(
                InventoryMovementType.Receive,
                0,
                0,
                0,
                0,
                0,
                "PO-1001",
                "Stock receipt");

        // Assert
        action.Should()
            .Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Create_WithNegativeQuantity_ThrowsBusinessRuleViolationException()
    {
        // Act
        var action = () =>
            InventoryTransaction.Create(
                InventoryMovementType.Receive,
                -10,
                0,
                0,
                0,
                0,
                "PO-1001",
                "Stock receipt");

        // Assert
        action.Should()
            .Throw<BusinessRuleViolationException>();
    }

    #endregion

    #region Available Quantity Validation Tests

    [Fact]
    public void Create_WithNegativeAvailableQuantityBefore_ThrowsBusinessRuleViolationException()
    {
        // Act
        var action = () =>
            InventoryTransaction.Create(
                InventoryMovementType.Receive,
                10,
                -1,
                9,
                0,
                0,
                "PO-1001",
                "Stock receipt");

        // Assert
        action.Should()
            .Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Create_WithNegativeAvailableQuantityAfter_ThrowsBusinessRuleViolationException()
    {
        // Act
        var action = () =>
            InventoryTransaction.Create(
                InventoryMovementType.Receive,
                10,
                10,
                -1,
                0,
                0,
                "PO-1001",
                "Stock receipt");

        // Assert
        action.Should()
            .Throw<BusinessRuleViolationException>();
    }

    #endregion

    #region Reserved Quantity Validation Tests

    [Fact]
    public void Create_WithNegativeReservedQuantityBefore_ThrowsBusinessRuleViolationException()
    {
        // Act
        var action = () =>
            InventoryTransaction.Create(
                InventoryMovementType.Reserve,
                10,
                100,
                90,
                -1,
                9,
                "ORDER-1001",
                "Stock reservation");

        // Assert
        action.Should()
            .Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Create_WithNegativeReservedQuantityAfter_ThrowsBusinessRuleViolationException()
    {
        // Act
        var action = () =>
            InventoryTransaction.Create(
                InventoryMovementType.Reserve,
                10,
                100,
                90,
                10,
                -1,
                "ORDER-1001",
                "Stock reservation");

        // Assert
        action.Should()
            .Throw<BusinessRuleViolationException>();
    }

    #endregion

    #region Reference Validation Tests

    [Fact]
    public void Create_WithEmptyReference_ThrowsArgumentException()
    {
        // Act
        var action = () =>
            InventoryTransaction.Create(
                InventoryMovementType.Receive,
                10,
                0,
                10,
                0,
                0,
                string.Empty,
                "Stock receipt");

        // Assert
        action.Should()
            .Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithWhitespaceReference_ThrowsArgumentException()
    {
        // Act
        var action = () =>
            InventoryTransaction.Create(
                InventoryMovementType.Receive,
                10,
                0,
                10,
                0,
                0,
                "   ",
                "Stock receipt");

        // Assert
        action.Should()
            .Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithReferenceContainingWhitespace_TrimsReference()
    {
        // Act
        var transaction = InventoryTransaction.Create(
            InventoryMovementType.Receive,
            10,
            0,
            10,
            0,
            0,
            "  PO-1001  ",
            "Stock receipt");

        // Assert
        transaction.Reference
            .Should()
            .Be("PO-1001");
    }

    #endregion

    #region Reason Validation Tests

    [Fact]
    public void Create_WithEmptyReason_ThrowsArgumentException()
    {
        // Act
        var action = () =>
            InventoryTransaction.Create(
                InventoryMovementType.Receive,
                10,
                0,
                10,
                0,
                0,
                "PO-1001",
                string.Empty);

        // Assert
        action.Should()
            .Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithWhitespaceReason_ThrowsArgumentException()
    {
        // Act
        var action = () =>
            InventoryTransaction.Create(
                InventoryMovementType.Receive,
                10,
                0,
                10,
                0,
                0,
                "PO-1001",
                "   ");

        // Assert
        action.Should()
            .Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithReasonContainingWhitespace_TrimsReason()
    {
        // Act
        var transaction = InventoryTransaction.Create(
            InventoryMovementType.Receive,
            10,
            0,
            10,
            0,
            0,
            "PO-1001",
            "  Stock receipt  ");

        // Assert
        transaction.Reason
            .Should()
            .Be("Stock receipt");
    }

    #endregion

    #region Movement Type Tests

    [Fact]
    public void Create_WithMovementType_PreservesMovementType()
    {
        // Arrange
        var movementType = InventoryMovementType.Reserve;

        // Act
        var transaction = InventoryTransaction.Create(
            movementType,
            20,
            100,
            80,
            0,
            20,
            "ORDER-1001",
            "Order reservation");

        // Assert
        transaction.MovementType
            .Should()
            .Be(movementType);
    }

    #endregion

    #region Quantity Transition Tests

    [Fact]
    public void Create_WithReceiveMovement_StoresCorrectQuantityTransition()
    {
        // Act
        var transaction = InventoryTransaction.Create(
            InventoryMovementType.Receive,
            50,
            100,
            150,
            0,
            0,
            "PO-1001",
            "Stock received");

        // Assert
        transaction.AvailableQuantityBefore
            .Should()
            .Be(100);

        transaction.AvailableQuantityAfter
            .Should()
            .Be(150);

        transaction.ReservedQuantityBefore
            .Should()
            .Be(0);

        transaction.ReservedQuantityAfter
            .Should()
            .Be(0);
    }

    [Fact]
    public void Create_WithReservationMovement_StoresCorrectQuantityTransition()
    {
        // Act
        var transaction = InventoryTransaction.Create(
            InventoryMovementType.Reserve,
            20,
            100,
            80,
            0,
            20,
            "ORDER-1001",
            "Order reservation");

        // Assert
        transaction.AvailableQuantityBefore
            .Should()
            .Be(100);

        transaction.AvailableQuantityAfter
            .Should()
            .Be(80);

        transaction.ReservedQuantityBefore
            .Should()
            .Be(0);

        transaction.ReservedQuantityAfter
            .Should()
            .Be(20);
    }

    #endregion

    #region Test Helpers

    private static InventoryTransaction CreateValidTransaction()
    {
        return InventoryTransaction.Create(
            InventoryMovementType.Receive,
            quantity: 100,
            availableQuantityBefore: 0,
            availableQuantityAfter: 100,
            reservedQuantityBefore: 0,
            reservedQuantityAfter: 0,
            reference: "PO-1001",
            reason: "Initial stock receipt");
    }

    #endregion
}

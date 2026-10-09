using EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate;
using EnterpriseOrderManagementSystem.Infrastructure.Persistence.Repositories;
using EnterpriseOrderManagementSystem.Infrastructure.Tests.Database;

namespace EnterpriseOrderManagementSystem.Infrastructure.Tests.Repositories;

[Collection("SQL Server")]
public sealed class InventoryRepositoryTests
{
    private readonly SqlServerFixture _fixture;

    public InventoryRepositoryTests(
        SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    #region Add And Get By Id With Transactions

    [Fact]
    public async Task AddAsync_ThenGetByIdAsync_ShouldReturnInventoryWithTransactions()
    {
        // Arrange
        await using var context =
            _fixture.CreateDbContext();

        var repository =
            new InventoryRepository(context);

        var inventoryId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();

        var inventory =
            Inventory.Create(
                inventoryId,
                productId,
                warehouseId,
                reorderLevel: 10,
                maximumQuantity: 100);

        // First transaction: receive 50 units.
        inventory.ReceiveStock(
            quantity: 50,
            reference: "PO-TEST-001",
            reason: "Initial stock receipt for integration testing");

        // Second transaction: reserve 10 units.
        inventory.ReserveStock(
            quantity: 10,
            reference: "ORDER-TEST-001",
            reason: "Reserve stock for an integration test order");

        // Capture expected values before persistence.
        var expectedTransactions = inventory.Transactions
            .Select(transaction => new
            {
                transaction.Id,
                transaction.MovementType,
                transaction.Quantity,
                transaction.AvailableQuantityBefore,
                transaction.AvailableQuantityAfter,
                transaction.ReservedQuantityBefore,
                transaction.ReservedQuantityAfter,
                transaction.Reference,
                transaction.Reason,
                transaction.OccurredOnUtc
            })
            .ToList();

        var expectedAvailableQuantity =
            inventory.AvailableQuantity;

        var expectedReservedQuantity =
            inventory.ReservedQuantity;

        var expectedTotalQuantity =
            inventory.TotalQuantity;

        // Act
        await repository.AddAsync(inventory);

        await context.SaveChangesAsync();

        // Use a fresh DbContext to verify actual database persistence.
        await using var verificationContext =
            _fixture.CreateDbContext();

        var verificationRepository =
            new InventoryRepository(verificationContext);

        var result =
            await verificationRepository.GetByIdAsync(
                inventory.Id);

        // Assert: inventory was persisted and retrieved.
        Assert.NotNull(result);

        Assert.Equal(inventoryId, result.Id);
        Assert.Equal(productId, result.ProductId);
        Assert.Equal(warehouseId, result.WarehouseId);

        // Assert: inventory quantities were persisted.
        Assert.Equal(
            expectedAvailableQuantity,
            result.AvailableQuantity);

        Assert.Equal(
            expectedReservedQuantity,
            result.ReservedQuantity);

        Assert.Equal(
            expectedTotalQuantity,
            result.TotalQuantity);

        // Assert: transactions were eagerly loaded.
        Assert.Equal(
            expectedTransactions.Count,
            result.Transactions.Count);

        Assert.Equal(2, result.Transactions.Count);

        // Assert: every transaction was persisted with its details.
        foreach (var expectedTransaction in expectedTransactions)
        {
            var actualTransaction = Assert.Single(
                result.Transactions.Where(
                    transaction =>
                        transaction.Id == expectedTransaction.Id));

            Assert.Equal(
                expectedTransaction.MovementType,
                actualTransaction.MovementType);

            Assert.Equal(
                expectedTransaction.Quantity,
                actualTransaction.Quantity);

            Assert.Equal(
                expectedTransaction.AvailableQuantityBefore,
                actualTransaction.AvailableQuantityBefore);

            Assert.Equal(
                expectedTransaction.AvailableQuantityAfter,
                actualTransaction.AvailableQuantityAfter);

            Assert.Equal(
                expectedTransaction.ReservedQuantityBefore,
                actualTransaction.ReservedQuantityBefore);

            Assert.Equal(
                expectedTransaction.ReservedQuantityAfter,
                actualTransaction.ReservedQuantityAfter);

            Assert.Equal(
                expectedTransaction.Reference,
                actualTransaction.Reference);

            Assert.Equal(
                expectedTransaction.Reason,
                actualTransaction.Reason);

            Assert.Equal(
                expectedTransaction.OccurredOnUtc,
                actualTransaction.OccurredOnUtc);

            Assert.Equal(
                result.Id,
                actualTransaction.Inventory.Id);
        }
    }

    #endregion

    #region Get By Id When Inventory Does Not Exist

    [Fact]
    public async Task GetByIdAsync_WhenInventoryDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        await using var context =
            _fixture.CreateDbContext();

        var repository =
            new InventoryRepository(context);

        var inventoryId = Guid.NewGuid();

        // Act
        var result =
            await repository.GetByIdAsync(inventoryId);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Exists

    [Fact]
    public async Task ExistsAsync_WhenInventoryExists_ShouldReturnTrue()
    {
        // Arrange
        await using var context =
            _fixture.CreateDbContext();

        var repository =
            new InventoryRepository(context);

        var inventory =
            Inventory.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                reorderLevel: 5,
                maximumQuantity: 50);

        await repository.AddAsync(inventory);

        await context.SaveChangesAsync();

        // Act
        var exists =
            await repository.ExistsAsync(inventory.Id);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_WhenInventoryDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        await using var context =
            _fixture.CreateDbContext();

        var repository =
            new InventoryRepository(context);

        // Act
        var exists =
            await repository.ExistsAsync(Guid.NewGuid());

        // Assert
        Assert.False(exists);
    }

    #endregion
}
using EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.PaymentAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.ProductAggregate;
using EnterpriseOrderManagementSystem.Domain.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Common.ValueObjects;
using EnterpriseOrderManagementSystem.Domain.Enums;
using EnterpriseOrderManagementSystem.Domain.Services;
using Xunit;

namespace EnterpriseOrderManagementSystem.Domain.Tests.Services
{
    public sealed class InventoryAllocationDomainServiceTests
    {
        private readonly InventoryAllocationDomainService _service = new();

        [Fact]
        public void Allocate_ShouldCreateAllocationForEachOrderItem()
        {
            // Arrange
            var product1Id = Guid.NewGuid();
            var product2Id = Guid.NewGuid();

            var order = Order.Create(Guid.NewGuid());

            order.AddItem(
                product1Id,
                "Laptop",
                Money.Create(1000m, "USD"),
                2);

            order.AddItem(
                product2Id,
                "Monitor",
                Money.Create(500m, "USD"),
                3);

            var inventory1 = Inventory.Create(
                Guid.NewGuid(),
                product1Id,
                Guid.NewGuid(),
                reorderLevel: 5,
                maximumQuantity: 100);

            var inventory2 = Inventory.Create(
                Guid.NewGuid(),
                product2Id,
                Guid.NewGuid(),
                reorderLevel: 5,
                maximumQuantity: 100);

            inventory1.ReceiveStock(
                10,
                "PO-001",
                "Initial stock");

            inventory2.ReceiveStock(
                20,
                "PO-002",
                "Initial stock");

            var inventories = new[] { inventory1, inventory2 };

            // Act
            var result = _service.Allocate(
                order,
                inventories);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            var allocation1 = result.Single(
                x => x.ProductId == product1Id);

            var allocation2 = result.Single(
                x => x.ProductId == product2Id);

            Assert.Equal(inventory1.Id, allocation1.InventoryId);
            Assert.Equal(2, allocation1.Quantity);

            Assert.Equal(inventory2.Id, allocation2.InventoryId);
            Assert.Equal(3, allocation2.Quantity);
        }

        [Fact]
        public void Allocate_ShouldThrow_WhenOrderIsNull()
        {
            // Arrange
            var inventories = Array.Empty<Inventory>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _service.Allocate(
                    null!,
                    inventories));
        }

        [Fact]
        public void Allocate_ShouldThrow_WhenInventoriesAreNull()
        {
            // Arrange
            var order = Order.Create(Guid.NewGuid());

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _service.Allocate(
                    order,
                    null!));
        }

        [Fact]
        public void Allocate_ShouldThrow_WhenOrderHasNoItems()
        {
            // Arrange
            var order = Order.Create(Guid.NewGuid());

            var inventories = Array.Empty<Inventory>();

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleViolationException>(() =>
                _service.Allocate(
                    order,
                    inventories));

            Assert.Contains(
                "at least one item",
                exception.Message,
                StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Allocate_ShouldThrow_WhenInventoryDoesNotExist()
        {
            // Arrange
            var productId = Guid.NewGuid();

            var order = Order.Create(Guid.NewGuid());

            order.AddItem(
                productId,
                "Laptop",
                Money.Create(1000m, "USD"),
                2);

            var inventories = Array.Empty<Inventory>();

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleViolationException>(() =>
                _service.Allocate(
                    order,
                    inventories));

            Assert.Contains(
                "No inventory was found",
                exception.Message);
        }

        [Fact]
        public void Allocate_ShouldThrow_WhenInventoryIsInsufficient()
        {
            // Arrange
            var productId = Guid.NewGuid();

            var order = Order.Create(Guid.NewGuid());

            order.AddItem(
                productId,
                "Laptop",
                Money.Create(1000m, "USD"),
                10);

            var inventory = Inventory.Create(
                Guid.NewGuid(),
                productId,
                Guid.NewGuid(),
                reorderLevel: 2,
                maximumQuantity: 100);

            inventory.ReceiveStock(
                5,
                "PO-001",
                "Initial stock");

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleViolationException>(() =>
                _service.Allocate(
                    order,
                    new[] { inventory }));

            Assert.Contains(
                "Insufficient inventory",
                exception.Message);

            Assert.Contains(
                "Required: 10",
                exception.Message);

            Assert.Contains(
                "Available: 5",
                exception.Message);
        }

        [Fact]
        public void Allocate_ShouldNotModifyInventory()
        {
            // Arrange
            var productId = Guid.NewGuid();

            var order = Order.Create(Guid.NewGuid());

            order.AddItem(
                productId,
                "Laptop",
                Money.Create(1000m, "USD"),
                2);

            var inventory = Inventory.Create(
                Guid.NewGuid(),
                productId,
                Guid.NewGuid(),
                reorderLevel: 2,
                maximumQuantity: 100);

            inventory.ReceiveStock(
                10,
                "PO-001",
                "Initial stock");

            var availableBefore =
                inventory.AvailableQuantity;

            var reservedBefore =
                inventory.ReservedQuantity;

            // Act
            _service.Allocate(
                order,
                new[] { inventory });

            // Assert
            Assert.Equal(
                availableBefore,
                inventory.AvailableQuantity);

            Assert.Equal(
                reservedBefore,
                inventory.ReservedQuantity);
        }
    }
}
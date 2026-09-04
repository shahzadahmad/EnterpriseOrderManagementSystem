using EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.PaymentAggregate;
using EnterpriseOrderManagementSystem.Domain.Common.ValueObjects;
using EnterpriseOrderManagementSystem.Domain.Enums;
using EnterpriseOrderManagementSystem.Domain.Services;

namespace EnterpriseOrderManagementSystem.Domain.Tests.Services
{
    public sealed class OrderFulfillmentDomainServiceTests
    {
        private readonly OrderFulfillmentDomainService _service = new();

        [Fact]
        public void CanFulfill_ShouldReturnTrue_WhenAllConditionsAreSatisfied()
        {
            // Arrange
            var productId = Guid.NewGuid();

            var order = Order.Create(Guid.NewGuid());

            order.AddItem(
                productId,
                "Laptop",
                Money.Create(1000m, "USD"),
                2);

            order.Confirm();

            var payment = Payment.Create(
                Guid.NewGuid(),
                order.Id,
                2000m,
                "USD",
                PaymentMethod.CreditCard,
                PaymentProvider.Stripe);

            payment.Authorize(
                "AUTH-001",
                "AUTH-CODE-001");

            payment.Capture(
                "CAPTURE-001");

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

            // Act
            var result = _service.CanFulfill(
                order,
                payment,
                new[] { inventory });

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanFulfill_ShouldReturnFalse_WhenOrderIsNotConfirmed()
        {
            // Arrange
            var productId = Guid.NewGuid();

            var order = Order.Create(Guid.NewGuid());

            order.AddItem(
                productId,
                "Laptop",
                Money.Create(1000m, "USD"),
                1);

            var payment = Payment.Create(
                Guid.NewGuid(),
                order.Id,
                1000m,
                "USD",
                PaymentMethod.CreditCard,
                PaymentProvider.Stripe);

            payment.Authorize(
                "AUTH-001",
                "AUTH-CODE-001");

            payment.Capture(
                "CAPTURE-001");

            var inventory = CreateInventoryWithStock(
                productId,
                10);

            // Act
            var result = _service.CanFulfill(
                order,
                payment,
                new[] { inventory });

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CanFulfill_ShouldReturnFalse_WhenPaymentIsPending()
        {
            // Arrange
            var productId = Guid.NewGuid();

            var order = CreateConfirmedOrder(
                productId,
                1);

            var payment = Payment.Create(
                Guid.NewGuid(),
                order.Id,
                1000m,
                "USD",
                PaymentMethod.CreditCard,
                PaymentProvider.Stripe);

            var inventory = CreateInventoryWithStock(
                productId,
                10);

            // Act
            var result = _service.CanFulfill(
                order,
                payment,
                new[] { inventory });

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CanFulfill_ShouldReturnTrue_WhenPaymentIsSettled()
        {
            // Arrange
            var productId = Guid.NewGuid();

            var order = CreateConfirmedOrder(
                productId,
                1);

            var payment = Payment.Create(
                Guid.NewGuid(),
                order.Id,
                1000m,
                "USD",
                PaymentMethod.CreditCard,
                PaymentProvider.Stripe);

            payment.Authorize(
                "AUTH-001",
                "AUTH-CODE-001");

            payment.Capture(
                "CAPTURE-001");

            payment.Settle(
                "SETTLEMENT-001");

            var inventory = CreateInventoryWithStock(
                productId,
                10);

            // Act
            var result = _service.CanFulfill(
                order,
                payment,
                new[] { inventory });

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanFulfill_ShouldReturnFalse_WhenRequiredInventoryDoesNotExist()
        {
            // Arrange
            var productId = Guid.NewGuid();

            var order = CreateConfirmedOrder(
                productId,
                2);

            var payment = CreateCapturedPayment(
                order.Id,
                2000m);

            // Act
            var result = _service.CanFulfill(
                order,
                payment,
                Array.Empty<Inventory>());

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CanFulfill_ShouldReturnFalse_WhenInventoryIsInsufficient()
        {
            // Arrange
            var productId = Guid.NewGuid();

            var order = CreateConfirmedOrder(
                productId,
                10);

            var payment = CreateCapturedPayment(
                order.Id,
                10000m);

            var inventory = CreateInventoryWithStock(
                productId,
                5);

            // Act
            var result = _service.CanFulfill(
                order,
                payment,
                new[] { inventory });

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CanFulfill_ShouldReturnFalse_WhenPaymentIsCancelled()
        {
            // Arrange
            var productId = Guid.NewGuid();

            var order = CreateConfirmedOrder(
                productId,
                1);

            var payment = Payment.Create(
                Guid.NewGuid(),
                order.Id,
                1000m,
                "USD",
                PaymentMethod.CreditCard,
                PaymentProvider.Stripe);

            payment.Cancel("Invalid Credit Card");

            var inventory = CreateInventoryWithStock(
                productId,
                10);

            // Act
            var result = _service.CanFulfill(
                order,
                payment,
                new[] { inventory });

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CanFulfill_ShouldThrow_WhenOrderIsNull()
        {
            // Arrange
            var payment = Payment.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                1000m,
                "USD",
                PaymentMethod.CreditCard,
                PaymentProvider.Stripe);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _service.CanFulfill(
                    null!,
                    payment,
                    Array.Empty<Inventory>()));
        }

        [Fact]
        public void CanFulfill_ShouldThrow_WhenPaymentIsNull()
        {
            // Arrange
            var order = Order.Create(Guid.NewGuid());

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _service.CanFulfill(
                    order,
                    null!,
                    Array.Empty<Inventory>()));
        }

        [Fact]
        public void CanFulfill_ShouldThrow_WhenInventoriesAreNull()
        {
            // Arrange
            var order = Order.Create(Guid.NewGuid());

            var payment = Payment.Create(
                Guid.NewGuid(),
                order.Id,
                1000m,
                "USD",
                PaymentMethod.CreditCard,
                PaymentProvider.Stripe);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _service.CanFulfill(
                    order,
                    payment,
                    null!));
        }

        private static Order CreateConfirmedOrder(
            Guid productId,
            int quantity)
        {
            var order = Order.Create(
                Guid.NewGuid());

            order.AddItem(
                productId,
                "Test Product",
                Money.Create(
                    1000m,
                    "USD"),
                quantity);

            order.Confirm();

            return order;
        }

        private static Payment CreateCapturedPayment(
            Guid orderId,
            decimal amount)
        {
            var payment = Payment.Create(
                Guid.NewGuid(),
                orderId,
                amount,
                "USD",
                PaymentMethod.CreditCard,
                PaymentProvider.Stripe);

            payment.Authorize(
                "AUTH-001",
                "AUTH-CODE-001");

            payment.Capture(
                "CAPTURE-001");

            return payment;
        }

        private static Inventory CreateInventoryWithStock(
            Guid productId,
            int quantity)
        {
            var inventory = Inventory.Create(
                Guid.NewGuid(),
                productId,
                Guid.NewGuid(),
                reorderLevel: 2,
                maximumQuantity: 100);

            inventory.ReceiveStock(
                quantity,
                "PO-001",
                "Test stock");

            return inventory;
        }
    }
}

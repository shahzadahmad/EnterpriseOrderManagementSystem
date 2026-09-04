using EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.ProductAggregate;
using EnterpriseOrderManagementSystem.Domain.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Common.ValueObjects;
using EnterpriseOrderManagementSystem.Domain.Services;

namespace EnterpriseOrderManagementSystem.Domain.Tests.Services
{
    public sealed class OrderPricingDomainServiceTests
    {
        private readonly OrderPricingDomainService _service = new();

        [Fact]
        public void Calculate_ShouldReturnCorrectSubtotal()
        {
            // Arrange
            var product = Product.Create(
                "SKU-001",
                "Laptop",
                "Test laptop",
                Money.Create(1000m, "USD"));            

            var order = Order.Create(
                Guid.NewGuid());

            order.AddItem(
                product.Id,
                "Laptop",
                Money.Create(1000m, "USD"),
                2);            

            // Act
            var result = _service.Calculate(
                order,
                new[] { product });

            // Assert
            Assert.Equal(
                2000m,
                result.SubTotal);

            Assert.Equal(
                0m,
                result.DiscountAmount);

            Assert.Equal(
                0m,
                result.TaxAmount);

            Assert.Equal(
                2000m,
                result.TotalAmount);
        }

        [Fact]
        public void Calculate_ShouldCalculateMultipleOrderItems()
        {
            // Arrange
            var laptop = Product.Create(
                "SKU-LAPTOP",
                "Laptop",
                "Laptop",
                Money.Create(1000m, "USD"));

            var monitor = Product.Create(
                "SKU-MONITOR",
                "Monitor",
                "Monitor",
                Money.Create(500m, "USD"));

            var laptopId = laptop.Id;
            var monitorId = monitor.Id;

            var order = Order.Create(
                Guid.NewGuid());

            order.AddItem(
                laptopId,
                "Laptop",
                Money.Create(1000m, "USD"),
                2);

            order.AddItem(
                monitorId,
                "Monitor",
                Money.Create(500m, "USD"),
                3);
            

            // Act
            var result = _service.Calculate(
                order,
                new[] { laptop, monitor });

            // Assert
            Assert.Equal(
                3500m,
                result.SubTotal);

            Assert.Equal(
                0m,
                result.DiscountAmount);

            Assert.Equal(
                0m,
                result.TaxAmount);

            Assert.Equal(
                3500m,
                result.TotalAmount);
        }

        [Fact]
        public void Calculate_ShouldThrow_WhenOrderIsNull()
        {
            // Arrange
            var products = Array.Empty<Product>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _service.Calculate(
                    null!,
                    products));
        }

        [Fact]
        public void Calculate_ShouldThrow_WhenProductsAreNull()
        {
            // Arrange
            var order = Order.Create(
                Guid.NewGuid());

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _service.Calculate(
                    order,
                    null!));
        }

        [Fact]
        public void Calculate_ShouldThrow_WhenOrderHasNoItems()
        {
            // Arrange
            var order = Order.Create(
                Guid.NewGuid());

            // Act & Assert
            var exception =
                Assert.Throws<BusinessRuleViolationException>(() =>
                    _service.Calculate(
                        order,
                        Array.Empty<Product>()));

            Assert.Contains(
                "at least one item",
                exception.Message,
                StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Calculate_ShouldThrow_WhenProductIsMissing()
        {
            // Arrange
            var productId = Guid.NewGuid();

            var order = Order.Create(
                Guid.NewGuid());

            order.AddItem(
                productId,
                "Laptop",
                Money.Create(1000m, "USD"),
                1);

            var products = Array.Empty<Product>();

            // Act & Assert
            var exception =
                Assert.Throws<BusinessRuleViolationException>(() =>
                    _service.Calculate(
                        order,
                        products));

            Assert.Contains(
                "was not found",
                exception.Message,
                StringComparison.OrdinalIgnoreCase);

            Assert.Contains(
                productId.ToString(),
                exception.Message);
        }

        [Fact]
        public void Calculate_ShouldNotModifyOrder()
        {
            // Arrange
            var product = Product.Create(
                "SKU-001",
                "Laptop",
                "Laptop",
                Money.Create(1000m, "USD"));

            var productId = product.Id;

            var order = Order.Create(
                Guid.NewGuid());

            order.AddItem(
                productId,
                "Laptop",
                Money.Create(1000m, "USD"),
                2);

            var totalBefore =
                order.TotalAmount.Amount;

            // Act
            _service.Calculate(
                order,
                new[] { product });

            // Assert
            Assert.Equal(
                totalBefore,
                order.TotalAmount.Amount);
        }

        [Fact]
        public void Calculate_ShouldReturnZeroDiscountAndTax_WhenNoPoliciesExist()
        {
            // Arrange
            var product = Product.Create(
                "SKU-001",
                "Laptop",
                "Laptop",
                Money.Create(1500m, "USD"));

            var productId = product.Id;

            var order = Order.Create(
                Guid.NewGuid());

            order.AddItem(
                productId,
                "Laptop",
                Money.Create(1500m, "USD"),
                2);

            // Act
            var result = _service.Calculate(
                order,
                new[] { product });

            // Assert
            Assert.Equal(3000m, result.SubTotal);
            Assert.Equal(0m, result.DiscountAmount);
            Assert.Equal(0m, result.TaxAmount);
            Assert.Equal(3000m, result.TotalAmount);
        }
    }
}

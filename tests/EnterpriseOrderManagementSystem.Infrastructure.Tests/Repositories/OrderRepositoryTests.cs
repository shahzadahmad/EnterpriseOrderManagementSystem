using EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate;
using EnterpriseOrderManagementSystem.Domain.Common.ValueObjects;
using EnterpriseOrderManagementSystem.Infrastructure.Persistence.Repositories;
using EnterpriseOrderManagementSystem.Infrastructure.Tests.Database;
using Xunit;

namespace EnterpriseOrderManagementSystem.Infrastructure.Tests.Repositories;

[Collection("SQL Server")]
public sealed class OrderRepositoryTests
{
    private readonly SqlServerFixture _fixture;

    public OrderRepositoryTests(
        SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    #region Add And Get By Id

    [Fact]
    public async Task AddAsync_ThenGetByIdAsync_ShouldReturnOrderWithItems()
    {
        // Arrange
        await using var context =
            _fixture.CreateDbContext();

        var repository =
            new OrderRepository(context);

        var customerId = Guid.NewGuid();

        var firstProductId = Guid.NewGuid();
        var secondProductId = Guid.NewGuid();

        var order =
            Order.Create(customerId);

        order.AddItem(
            firstProductId,
            "Test Laptop",
            Money.Create(1500m, "USD"),
            2);

        order.AddItem(
            secondProductId,
            "Test Monitor",
            Money.Create(400m, "USD"),
            1);

        // Capture expected values before persistence.
        var expectedItems = order.Items
            .Select(item => new
            {
                item.ProductId,
                item.ProductName,
                item.Quantity,
                UnitPriceAmount = item.UnitPrice.Amount,
                UnitPriceCurrency = item.UnitPrice.Currency,
                LineTotalAmount = item.LineTotal.Amount,
                LineTotalCurrency = item.LineTotal.Currency
            })
            .ToList();

        var expectedTotalAmount = order.TotalAmount.Amount;
        var expectedTotalCurrency = order.TotalAmount.Currency;

        // Act
        await repository.AddAsync(order);

        await context.SaveChangesAsync();

        // Verify persistence using a fresh DbContext.
        await using var verificationContext =
            _fixture.CreateDbContext();

        var verificationRepository =
            new OrderRepository(verificationContext);

        var result =
            await verificationRepository.GetByIdAsync(
                order.Id);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(
            order.Id,
            result.Id);

        Assert.Equal(
            customerId,
            result.CustomerId);

        Assert.Equal(
            order.OrderNumber,
            result.OrderNumber);

        Assert.Equal(
            expectedItems.Count,
            result.Items.Count);

        Assert.Equal(
            expectedTotalAmount,
            result.TotalAmount.Amount);

        Assert.Equal(
            expectedTotalCurrency,
            result.TotalAmount.Currency);

        foreach (var expectedItem in expectedItems)
        {
            var actualItem = Assert.Single(
                result.Items.Where(item =>
                    item.ProductId == expectedItem.ProductId));

            Assert.Equal(
                expectedItem.ProductName,
                actualItem.ProductName);

            Assert.Equal(
                expectedItem.Quantity,
                actualItem.Quantity);

            Assert.Equal(
                expectedItem.UnitPriceAmount,
                actualItem.UnitPrice.Amount);

            Assert.Equal(
                expectedItem.UnitPriceCurrency,
                actualItem.UnitPrice.Currency);

            Assert.Equal(
                expectedItem.LineTotalAmount,
                actualItem.LineTotal.Amount);

            Assert.Equal(
                expectedItem.LineTotalCurrency,
                actualItem.LineTotal.Currency);
        }
    }

    #endregion
}
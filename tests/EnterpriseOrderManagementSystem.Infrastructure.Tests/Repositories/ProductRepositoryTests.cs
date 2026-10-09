using EnterpriseOrderManagementSystem.Domain.Aggregates.ProductAggregate;
using EnterpriseOrderManagementSystem.Domain.Common.ValueObjects;
using EnterpriseOrderManagementSystem.Infrastructure.Persistence.Repositories;
using EnterpriseOrderManagementSystem.Infrastructure.Tests.Database;
using Xunit;

namespace EnterpriseOrderManagementSystem.Infrastructure.Tests.Repositories;

[Collection("SQL Server")]
public sealed class ProductRepositoryTests
{
    private readonly SqlServerFixture _fixture;

    public ProductRepositoryTests(
        SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    #region Add And Get By SKU

    [Fact]
    public async Task AddAsync_ThenGetBySkuAsync_ShouldReturnProduct()
    {
        await using var context =
            _fixture.CreateDbContext();

        var repository =
            new ProductRepository(context);

        var product =
            Product.Create(
                "TEST-SKU-001",
                "Test Laptop",
                "Integration test laptop",
                Money.Create(
                    1500m,
                    "USD"));

        await repository.AddAsync(product);

        await context.SaveChangesAsync();

        var result =
            await repository.GetBySkuAsync(
                "TEST-SKU-001");

        Assert.NotNull(result);

        Assert.Equal(
            product.Id,
            result.Id);

        Assert.Equal(
            "TEST-SKU-001",
            result.SKU);

        Assert.Equal(
            1500m,
            result.Price.Amount);

        Assert.Equal(
            "USD",
            result.Price.Currency);
    }

    #endregion

    #region Exists

    [Fact]
    public async Task ExistsBySkuAsync_ShouldReturnTrue()
    {
        await using var context =
            _fixture.CreateDbContext();

        var repository =
            new ProductRepository(context);

        var product =
            Product.Create(
                "TEST-SKU-002",
                "Test Monitor",
                "Integration test monitor",
                Money.Create(
                    400m,
                    "USD"));

        await repository.AddAsync(product);

        await context.SaveChangesAsync();

        var exists =
            await repository.ExistsBySkuAsync(
                "TEST-SKU-002");

        Assert.True(exists);
    }

    #endregion
}

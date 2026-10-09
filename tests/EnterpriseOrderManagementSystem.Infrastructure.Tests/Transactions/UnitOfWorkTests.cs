using EnterpriseOrderManagementSystem.Domain.Aggregates.ProductAggregate;
using EnterpriseOrderManagementSystem.Domain.Common.ValueObjects;
using EnterpriseOrderManagementSystem.Infrastructure.Persistence.Repositories;
using EnterpriseOrderManagementSystem.Infrastructure.Persistence.Transactions;
using EnterpriseOrderManagementSystem.Infrastructure.Tests.Database;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseOrderManagementSystem.Infrastructure.Tests.Transactions;

[Collection("SQL Server")]
public sealed class UnitOfWorkTests
{
    private readonly SqlServerFixture _fixture;

    #region Constructor

    public UnitOfWorkTests(
        SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    #endregion

    #region Commit Transaction Tests

    [Fact]
    public async Task CommitTransactionAsync_ShouldPersistChanges()
    {
        // Arrange
        await using var context =
            _fixture.CreateDbContext();

        var unitOfWork =
            new UnitOfWork(context);

        var repository =
            new ProductRepository(context);

        var product =
            Product.Create(
                $"UOW-SKU-{Guid.NewGuid():N}",
                "UOW Test Product",
                "Transaction test product",
                Money.Create(
                    100m,
                    "USD"));

        // Act
        await unitOfWork.BeginTransactionAsync();

        await repository.AddAsync(product);

        await unitOfWork.SaveChangesAsync();

        await unitOfWork.CommitTransactionAsync();

        // Assert
        // Verify persistence using a separate DbContext.
        await using var verificationContext =
            _fixture.CreateDbContext();

        var exists =
            await verificationContext.Products
                .AnyAsync(
                    x => x.Id == product.Id);

        Assert.True(exists);
    }

    #endregion

    #region Rollback Transaction Tests

    [Fact]
    public async Task RollbackTransactionAsync_ShouldNotPersistChanges()
    {
        // Arrange
        await using var context =
            _fixture.CreateDbContext();

        var unitOfWork =
            new UnitOfWork(context);

        var repository =
            new ProductRepository(context);

        var product =
            Product.Create(
                $"UOW-ROLLBACK-{Guid.NewGuid():N}",
                "Rollback Test Product",
                "Product used to verify transaction rollback",
                Money.Create(
                    200m,
                    "USD"));

        // Act
        await unitOfWork.BeginTransactionAsync();

        await repository.AddAsync(product);

        // SaveChanges sends the INSERT to SQL Server,
        // but the active transaction has not been committed.
        await unitOfWork.SaveChangesAsync();

        // Rollback should undo the uncommitted database INSERT.
        await unitOfWork.RollbackTransactionAsync();

        // Assert
        // Use a separate DbContext to verify the database state.
        await using var verificationContext =
            _fixture.CreateDbContext();

        var exists =
            await verificationContext.Products
                .AnyAsync(
                    x => x.Id == product.Id);

        Assert.False(exists);
    }

    #endregion

}

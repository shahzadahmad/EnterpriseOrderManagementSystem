using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Application.Common.Models;
using EnterpriseOrderManagementSystem.Application.Features.Inventory.Commands;
using EnterpriseOrderManagementSystem.Application.Features.Inventory.Queries;
using EnterpriseOrderManagementSystem.Application.Abstractions.Persistence;
using EnterpriseOrderManagementSystem.Application.Tests.Common;
using InvtAggre = EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using Moq;

namespace EnterpriseOrderManagementSystem.Application.Tests.Features.Inventory;

public sealed class InventoryHandlerTests
{
    [Fact]
    public async Task ReceiveStock_ShouldIncreaseAvailableQuantity()
    {
        var inventory = TestData.Inventory();
        var repo = Repo(inventory);
        var result = await new ReceiveStockCommandHandler(repo.Object).Handle(
            new ReceiveStockCommand(inventory.Id, 5, "PO-2", "Restock"), CancellationToken.None);
        Assert.Equal(inventory.Id, result);
        Assert.Equal(5, inventory.AvailableQuantity);
    }

    [Fact]
    public async Task ReserveStock_ShouldMoveQuantityToReserved()
    {
        var inventory = TestData.InventoryWithStock(10);
        var repo = Repo(inventory);
        var result = await new ReserveStockCommandHandler(repo.Object).Handle(
            new ReserveStockCommand(inventory.Id, 3, "ORDER-2", "Reserve"), CancellationToken.None);
        Assert.Equal(inventory.Id, result);
        Assert.Equal(7, inventory.AvailableQuantity);
        Assert.Equal(3, inventory.ReservedQuantity);
    }

    [Fact]
    public async Task ReleaseReservedStock_ShouldReturnQuantityToAvailable()
    {
        var inventory = TestData.InventoryWithReservation(10, 3);
        var repo = Repo(inventory);
        var result = await new ReleaseReservedStockCommandHandler(repo.Object).Handle(
            new ReleaseReservedStockCommand(inventory.Id, 2, "ORDER-2", "Release"), CancellationToken.None);
        Assert.Equal(inventory.Id, result);
        Assert.Equal(9, inventory.AvailableQuantity);
        Assert.Equal(1, inventory.ReservedQuantity);
    }

    [Fact]
    public async Task CommitReservation_ShouldReduceReservedQuantity()
    {
        var inventory = TestData.InventoryWithReservation(10, 3);
        var repo = Repo(inventory);
        var result = await new CommitReservationCommandHandler(repo.Object).Handle(
            new CommitReservationCommand(inventory.Id, 2, "SHIP-2", "Commit"), CancellationToken.None);
        Assert.Equal(inventory.Id, result);
        Assert.Equal(1, inventory.ReservedQuantity);
        Assert.Equal(7, inventory.AvailableQuantity);
    }

    [Fact]
    public async Task AdjustStock_ShouldSetAvailableQuantity()
    {
        var inventory = TestData.InventoryWithStock(10);
        var repo = Repo(inventory);
        var result = await new AdjustStockCommandHandler(repo.Object).Handle(
            new AdjustStockCommand(inventory.Id, 7, "COUNT-2", "Stock count"), CancellationToken.None);
        Assert.Equal(inventory.Id, result);
        Assert.Equal(7, inventory.AvailableQuantity);
    }

    [Fact]
    public async Task DiscontinueInventory_ShouldMarkInventoryDiscontinued()
    {
        var inventory = TestData.Inventory();
        var repo = Repo(inventory);
        var result = await new DiscontinueInventoryCommandHandler(repo.Object).Handle(
            new DiscontinueInventoryCommand(inventory.Id, "Product retired"), CancellationToken.None);
        Assert.Equal(inventory.Id, result);
        Assert.True(inventory.IsDiscontinued);
    }

    [Fact]
    public async Task ReactivateInventory_ShouldMarkInventoryActive()
    {
        var inventory = TestData.Inventory();
        inventory.Discontinue("Maintenance");
        var repo = Repo(inventory);
        var result = await new ReactivateInventoryCommandHandler(repo.Object).Handle(
            new ReactivateInventoryCommand(inventory.Id), CancellationToken.None);
        Assert.Equal(inventory.Id, result);
        Assert.False(inventory.IsDiscontinued);
    }

    [Fact]
    public async Task GetInventoryById_ShouldReturnMappedDto()
    {
        var inventory = TestData.InventoryWithStock();
        var repo = Repo(inventory);
        var result = await new GetInventoryByIdQueryHandler(repo.Object).Handle(
            new GetInventoryByIdQuery(inventory.Id), CancellationToken.None);
        Assert.NotNull(result);
        Assert.Equal(inventory.Id, result!.Id);
        Assert.Equal(10, result.AvailableQuantity);
    }

    [Fact]
    public async Task GetInventoryById_ShouldReturnNullWhenMissing()
    {
        var id = Guid.NewGuid();
        var repo = new Mock<IInventoryRepository>();
        repo.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((InvtAggre.Inventory?)null);
        Assert.Null(await new GetInventoryByIdQueryHandler(repo.Object).Handle(
            new GetInventoryByIdQuery(id), CancellationToken.None));
    }

    [Fact]
    public async Task GetInventories_ShouldReturnPagedMappedResults()
    {
        var inventories = new[] { TestData.Inventory(), TestData.Inventory() };
        var repo = new Mock<IReadRepository<InvtAggre.Inventory>>();
        repo.Setup(x => x.CountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(21);
        repo.Setup(x => x.ListAsync(5, 5, It.IsAny<CancellationToken>())).ReturnsAsync(inventories);

        var result = await new GetInventoriesQueryHandler(repo.Object).Handle(
            new GetInventoriesQuery(new PaginationRequest(2, 5)), CancellationToken.None);

        Assert.Equal(21, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.PageNumber);
        repo.Verify(x => x.ListAsync(5, 5, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InventoryCommands_ShouldThrowNotFoundWhenInventoryDoesNotExist()
    {
        var id = Guid.NewGuid();
        var repo = new Mock<IInventoryRepository>();
        repo.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((InvtAggre.Inventory?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => new ReceiveStockCommandHandler(repo.Object)
            .Handle(new ReceiveStockCommand(id, 1, "R", "Reason"), CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(() => new ReserveStockCommandHandler(repo.Object)
            .Handle(new ReserveStockCommand(id, 1, "R", "Reason"), CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(() => new ReleaseReservedStockCommandHandler(repo.Object)
            .Handle(new ReleaseReservedStockCommand(id, 1, "R", "Reason"), CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(() => new CommitReservationCommandHandler(repo.Object)
            .Handle(new CommitReservationCommand(id, 1, "R", "Reason"), CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(() => new AdjustStockCommandHandler(repo.Object)
            .Handle(new AdjustStockCommand(id, 1, "R", "Reason"), CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(() => new DiscontinueInventoryCommandHandler(repo.Object)
            .Handle(new DiscontinueInventoryCommand(id, "Reason"), CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(() => new ReactivateInventoryCommandHandler(repo.Object)
            .Handle(new ReactivateInventoryCommand(id), CancellationToken.None));
    }

    private static Mock<IInventoryRepository> Repo(InvtAggre.Inventory inventory)
    {
        var repo = new Mock<IInventoryRepository>();
        repo.Setup(x => x.GetByIdAsync(inventory.Id, It.IsAny<CancellationToken>())).ReturnsAsync(inventory);
        return repo;
    }
}

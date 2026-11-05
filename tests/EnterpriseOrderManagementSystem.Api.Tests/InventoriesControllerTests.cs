using EnterpriseOrderManagementSystem.Application.Common.Models;
using ApplicationDtos =  EnterpriseOrderManagementSystem.Application.DTOs.Inventory;
using EnterpriseOrderManagementSystem.Application.Features.Inventory.Commands;
using EnterpriseOrderManagementSystem.Application.Features.Inventory.Queries;
using EnterpriseOrderManagementSystem.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using System.Net;
using System.Net.Http.Json;

namespace EnterpriseOrderManagementSystem.Api.Tests;

public sealed class InventoriesControllerTests
    : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;
    private readonly HttpClient _client;

    public InventoriesControllerTests(ApiTestFactory factory)
    {
        _factory = factory;
        _factory.SenderMock.Reset();

        _client = _factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
    }

    [Fact]
    public async Task Create_WithValidRequest_ShouldReturn201()
    {
        var inventoryId = Guid.NewGuid();

        _factory.SenderMock
            .Setup(sender => sender.Send(
                It.IsAny<CreateInventoryCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventoryId);

        var response = await _client.PostAsJsonAsync(
            "/api/Inventories",
            new
            {
                ProductId = Guid.NewGuid(),
                WarehouseId = Guid.NewGuid(),
                ReorderLevel = 10,
                MaximumQuantity = 500
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task GetById_WhenInventoryExists_ShouldReturn200()
    {
        // Arrange
        var inventoryId = Guid.NewGuid();

        var inventory = new ApplicationDtos.InventoryDto(
            Id: inventoryId,
            ProductId: Guid.NewGuid(),
            WarehouseId: Guid.NewGuid(),
            AvailableQuantity: 120,
            ReservedQuantity: 20,
            ReorderLevel: 10,
            MaximumQuantity: 500,
            IsDiscontinued: false,
            LastStockMovementUtc: DateTime.UtcNow,
            TotalQuantity: 140,
            RemainingCapacity: 360,
            HasRemainingCapacity: true,
            NeedsReorder: false,
            IsOutOfStock: false,
            Status: InventoryStatus.InStock,
            Transactions: Array.Empty<ApplicationDtos.InventoryTransactionDto>()
        );

        _factory.SenderMock
            .Setup(sender => sender.Send(
                It.Is<GetInventoryByIdQuery>(
                    q => q.InventoryId == inventoryId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventory);

        // Act
        var response = await _client.GetAsync(
            $"/api/Inventories/{inventoryId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WhenInventoryMissing_ShouldReturn404()
    {
        var inventoryId = Guid.NewGuid();

        _factory.SenderMock
            .Setup(sender => sender.Send(
                It.IsAny<GetInventoryByIdQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationDtos.InventoryDto?)null);

        var response = await _client.GetAsync($"/api/Inventories/{inventoryId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ReceiveStock_ShouldDispatchCommand()
    {
        var inventoryId = Guid.NewGuid();

        _factory.SenderMock
            .Setup(sender => sender.Send(
                It.Is<ReceiveStockCommand>(c => c.InventoryId == inventoryId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventoryId);

        var response = await _client.PostAsJsonAsync(
            $"/api/Inventories/{inventoryId}/receive",
            new
            {
                Quantity = 50,
                Reference = "PO-1001",
                Reason = "Purchase Order"
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        _factory.SenderMock.Verify(
            sender => sender.Send(
                It.Is<ReceiveStockCommand>(c => c.InventoryId == inventoryId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ReserveStock_ShouldDispatchCommand()
    {
        var inventoryId = Guid.NewGuid();

        _factory.SenderMock
            .Setup(sender => sender.Send(
                It.IsAny<ReserveStockCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventoryId);

        var response = await _client.PostAsJsonAsync(
            $"/api/Inventories/{inventoryId}/reserve",
            new
            {
                Quantity = 5,
                Reference = "ORD-1001",
                Reason = "Customer Order"
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ReleaseReservedStock_ShouldDispatchCommand()
    {
        var inventoryId = Guid.NewGuid();

        _factory.SenderMock
            .Setup(sender => sender.Send(
                It.IsAny<ReleaseReservedStockCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventoryId);

        var response = await _client.PostAsJsonAsync(
            $"/api/Inventories/{inventoryId}/release",
            new
            {
                Quantity = 2,
                Reference = "ORD-1001",
                Reason = "Order Cancelled"
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CommitReservation_ShouldDispatchCommand()
    {
        var inventoryId = Guid.NewGuid();

        _factory.SenderMock
            .Setup(sender => sender.Send(
                It.IsAny<CommitReservationCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventoryId);

        var response = await _client.PostAsJsonAsync(
            $"/api/Inventories/{inventoryId}/commit",
            new
            {
                Quantity = 5,
                Reference = "ORD-1001",
                Reason = "Shipment"
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AdjustStock_ShouldDispatchCommand()
    {
        var inventoryId = Guid.NewGuid();

        _factory.SenderMock
            .Setup(sender => sender.Send(
                It.IsAny<AdjustStockCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventoryId);

        var response = await _client.PatchAsJsonAsync(
            $"/api/Inventories/{inventoryId}/adjust",
            new
            {
                NewAvailableQuantity = 80,
                Reference = "AUDIT-001",
                Reason = "Stock Audit"
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Discontinue_ShouldDispatchCommand()
    {
        var inventoryId = Guid.NewGuid();

        _factory.SenderMock
            .Setup(sender => sender.Send(
                It.IsAny<DiscontinueInventoryCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventoryId);

        var response = await _client.PatchAsJsonAsync(
            $"/api/Inventories/{inventoryId}/discontinue",
            new { Reason = "Product retired" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Reactivate_ShouldDispatchCommand()
    {
        var inventoryId = Guid.NewGuid();

        _factory.SenderMock
            .Setup(sender => sender.Send(
                It.IsAny<ReactivateInventoryCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventoryId);

        var response = await _client.PatchAsync(
            $"/api/Inventories/{inventoryId}/reactivate",
            null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ShouldReturnPaginatedInventories()
    {
        var result = new PaginatedResult<ApplicationDtos.InventoryDto>(
            new[]
            {
            new ApplicationDtos.InventoryDto(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                100,
                10,
                20,
                500,
                true,
                DateTime.UtcNow,
                0,
                0,
                false,
                false,
                false,
                InventoryStatus.InStock,
                Array.Empty<ApplicationDtos.InventoryTransactionDto>())
            },
            PageNumber: 1,
            PageSize: 20,
            TotalCount: 1);

        _factory.SenderMock
            .Setup(sender => sender.Send(
                It.IsAny<GetInventoriesQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        var response = await _client.GetAsync(
            "/api/Inventories?pageNumber=1&pageSize=20");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
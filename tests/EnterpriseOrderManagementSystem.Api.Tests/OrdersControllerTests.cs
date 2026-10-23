
using EnterpriseOrderManagementSystem.Application.Common.Models;
using EnterpriseOrderManagementSystem.Application.DTOs.Orders;
using EnterpriseOrderManagementSystem.Application.Features.Orders.Commands;
using EnterpriseOrderManagementSystem.Application.Features.Orders.Queries;
using EnterpriseOrderManagementSystem.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Moq;

namespace EnterpriseOrderManagementSystem.Api.Tests;

public sealed class OrdersControllerTests
    : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;
    private readonly HttpClient _client;

    public OrdersControllerTests(ApiTestFactory factory)
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
    public async Task Create_WithValidRequest_ShouldReturn201AndLocation()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        _factory.SenderMock
            .Setup(sender => sender.Send(
                It.Is<CreateOrderCommand>(
                    command => command.CustomerId == customerId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderId);

        var request = new
        {
            CustomerId = customerId
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/Orders",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var body = await response.Content
            .ReadFromJsonAsync<OrderIdResponse>();

        Assert.NotNull(body);
        Assert.Equal(orderId, body.Id);

        _factory.SenderMock.Verify(
            sender => sender.Send(
                It.Is<CreateOrderCommand>(
                    command => command.CustomerId == customerId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetById_WhenOrderDoesNotExist_ShouldReturn404()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _factory.SenderMock
            .Setup(sender => sender.Send(
                It.Is<GetOrderByIdQuery>(
                    query => query.OrderId == orderId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderDto?)null);

        // Act
        var response = await _client.GetAsync(
            $"/api/Orders/{orderId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WhenOrderExists_ShouldReturn200AndOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var createdOnUtc = DateTime.UtcNow;

        var order = new OrderDto(
            orderId,
            customerId,
            "ORD-TEST-001",
            OrderStatus.Pending,
            0m,
            "USD",
            createdOnUtc,
            null, // CancelledOnUtc
            null, // CancellationReason
            Array.Empty<OrderItemDto>());

        _factory.SenderMock
            .Setup(sender => sender.Send(
                It.Is<GetOrderByIdQuery>(
                    query => query.OrderId == orderId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var response = await _client.GetAsync(
            $"/api/Orders/{orderId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<OrderDto>();

        Assert.NotNull(result);
        Assert.Equal(orderId, result.Id);
        Assert.Equal(customerId, result.CustomerId);
        Assert.Equal("ORD-TEST-001", result.OrderNumber);
        Assert.Equal(OrderStatus.Pending, result.Status);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task GetAll_WithValidPagination_ShouldReturnPaginatedOrders()
    {
        // Arrange
        var order1 = new OrderDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "ORD-TEST-001",
            OrderStatus.Pending,
            100m,
            "USD",
            DateTime.UtcNow,
            null, // CancelledOnUtc
            null, // CancellationReason
            Array.Empty<OrderItemDto>());

        var order2 = new OrderDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "ORD-TEST-002",
            OrderStatus.Confirmed,
            200m,
            "USD",
            DateTime.UtcNow,
            null, // CancelledOnUtc
            null, // CancellationReason
            Array.Empty<OrderItemDto>());

        var result = new PaginatedResult<OrderDto>(
            new[] { order1, order2 },
            PageNumber: 1,
            PageSize: 20,
            TotalCount: 2);

        _factory.SenderMock
            .Setup(sender => sender.Send(
                It.Is<GetOrdersQuery>(
                    query => query.Pagination.PageNumber == 1 &&
                             query.Pagination.PageSize == 20),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _client.GetAsync(
            "/api/Orders?pageNumber=1&pageSize=20");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<PaginatedResult<OrderDto>>();

        Assert.NotNull(body);
        Assert.Equal(1, body.PageNumber);
        Assert.Equal(20, body.PageSize);
        Assert.Equal(2, body.TotalCount);
        Assert.Equal(2, body.Items.Count);
        Assert.Equal(1, body.TotalPages);
        Assert.False(body.HasNextPage);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public async Task GetAll_WithInvalidPagination_ShouldReturn400(
        int pageNumber,
        int pageSize)
    {
        // Act
        var response = await _client.GetAsync(
            $"/api/Orders?pageNumber={pageNumber}&pageSize={pageSize}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        _factory.SenderMock.Verify(
            sender => sender.Send(
                It.IsAny<GetOrdersQuery>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Cancel_WithValidReason_ShouldSendCancellationCommand()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        const string reason = "Customer requested cancellation";

        _factory.SenderMock
            .Setup(sender => sender.Send(
                It.Is<CancelOrderCommand>(
                    command => command.OrderId == orderId &&
                               command.Reason == reason),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderId);

        // Act
        var response = await _client.PatchAsJsonAsync(
            $"/api/Orders/{orderId}/cancel",
            new { Reason = reason });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<OrderStatusResponse>();

        Assert.NotNull(body);
        Assert.Equal(orderId, body.Id);
        Assert.Equal("Cancelled", body.Status);

        _factory.SenderMock.Verify(
            sender => sender.Send(
                It.Is<CancelOrderCommand>(
                    command => command.OrderId == orderId &&
                               command.Reason == reason),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task AddItem_WithValidRequest_ShouldSendAddItemCommand()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        const int quantity = 3;

        _factory.SenderMock
            .Setup(sender => sender.Send(
                It.Is<AddOrderItemCommand>(
                    command => command.OrderId == orderId &&
                               command.ProductId == productId &&
                               command.Quantity == quantity),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderId);

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/Orders/{orderId}/items",
            new
            {
                ProductId = productId,
                Quantity = quantity
            });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        _factory.SenderMock.Verify(
            sender => sender.Send(
                It.Is<AddOrderItemCommand>(
                    command => command.OrderId == orderId &&
                               command.ProductId == productId &&
                               command.Quantity == quantity),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Confirm_ShouldSendConfirmCommand()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _factory.SenderMock
            .Setup(sender => sender.Send(
                It.Is<ConfirmOrderCommand>(
                    command => command.OrderId == orderId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderId);

        // Act
        var response = await _client.PatchAsync(
            $"/api/Orders/{orderId}/confirm",
            content: null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<OrderStatusResponse>();

        Assert.NotNull(body);
        Assert.Equal(orderId, body.Id);
        Assert.Equal("Confirmed", body.Status);
    }

    private sealed record OrderIdResponse(Guid Id);

    private sealed record OrderStatusResponse(
        Guid Id,
        string Status);
}


using EnterpriseOrderManagementSystem.Application.DTOs.Products;
using EnterpriseOrderManagementSystem.Application.Features.Products.Commands;
using EnterpriseOrderManagementSystem.Application.Features.Products.Queries;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using System.Net;
using System.Net.Http.Json;

namespace EnterpriseOrderManagementSystem.Api.Tests;

public sealed class ProductsControllerTests
    : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;
    private readonly HttpClient _client;

    public ProductsControllerTests(ApiTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
    }

    [Fact]
    public async Task Create_ShouldReturn201AndLocationHeader()
    {
        // Arrange
        var productId = Guid.NewGuid();

        _factory.SenderMock
            .Setup(sender => sender.Send(
                It.IsAny<CreateProductCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(productId);

        var request = new
        {
            SKU = "API-TEST-001",
            Name = "Integration Test Product",
            Description = "Product created through the API test",
            Price = 99.99m,
            Currency = "USD"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Products", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        _factory.SenderMock.Verify(
            sender => sender.Send(
                It.Is<CreateProductCommand>(command =>
                    command.SKU == request.SKU &&
                    command.Name == request.Name),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetById_WhenProductDoesNotExist_ShouldReturn404()
    {
        // Arrange
        var productId = Guid.NewGuid();

        _factory.SenderMock.Setup(
            sender => sender.Send(
                It.IsAny<GetProductByIdQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductDto?)null);

        // Act
        var response = await _client.GetAsync($"/api/Products/{productId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

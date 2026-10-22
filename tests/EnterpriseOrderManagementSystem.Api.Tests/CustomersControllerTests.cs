
using EnterpriseOrderManagementSystem.Application.Common.Models;
using EnterpriseOrderManagementSystem.Application.DTOs.Customers;
using EnterpriseOrderManagementSystem.Application.Features.Customers.Commands;
using EnterpriseOrderManagementSystem.Application.Features.Customers.Queries;
using EnterpriseOrderManagementSystem.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using System.Net;
using System.Net.Http.Json;

namespace EnterpriseOrderManagementSystem.Api.Tests;

public sealed class CustomersControllerTests
    : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;
    private readonly HttpClient _client;

    public CustomersControllerTests(ApiTestFactory factory)
    {
        _factory = factory;
        _factory.SenderMock.Reset();

        _client = _factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
    }

    private static object ValidCustomerRequest() => new
    {
        FirstName = "Ali",
        LastName = "Khan",
        Email = "ali.khan@example.com",
        PhoneCountryCode = "+92",
        PhoneNationalNumber = "3001234567",
        Street = "Main Street",
        City = "Islamabad",
        State = "ICT",
        Country = "Pakistan",
        PostalCode = "44000"
    };

    [Fact]
    public async Task Create_WithValidRequest_ShouldReturn201AndLocation()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        _factory.SenderMock.Setup(
            sender => sender.Send(
                It.IsAny<CreateCustomerCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customerId);

        // Act
        var response = await _client.PostAsJsonAsync("/api/Customers", ValidCustomerRequest());

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        _factory.SenderMock.Verify(
            sender => sender.Send(
                It.Is<CreateCustomerCommand>(command =>
                    command.FirstName == "Ali" &&
                    command.LastName == "Khan" &&
                    command.Email == "ali.khan@example.com"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Create_WithInvalidEmail_ShouldReturn400()
    {
        // Arrange
        var request = new
        {
            FirstName = "Ali",
            LastName = "Khan",
            Email = "not-an-email",
            PhoneCountryCode = "+92",
            PhoneNationalNumber = "3001234567",
            Street = "Main Street",
            City = "Islamabad",
            State = "ICT",
            Country = "Pakistan",
            PostalCode = "44000"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Customers", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        _factory.SenderMock.Verify(
            sender => sender.Send(
                It.IsAny<CreateCustomerCommand>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetById_WhenCustomerDoesNotExist_ShouldReturn404()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        _factory.SenderMock.Setup(
            sender => sender.Send(
                It.IsAny<GetCustomerByIdQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerDto?)null);

        // Act
        var response = await _client.GetAsync($"/api/Customers/{customerId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Activate_ShouldReturn200AndActiveStatus()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        _factory.SenderMock.Setup(
            sender => sender.Send(
                It.Is<ActivateCustomerCommand>(
                    command => command.CustomerId == customerId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customerId);

        // Act
        var response = await _client.PatchAsync($"/api/Customers/{customerId}/activate", content: null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<CustomerStatusResponse>();

        Assert.NotNull(body);
        Assert.Equal(customerId, body.Id);
        Assert.Equal("Active", body.Status);
    }

    [Fact]
    public async Task Deactivate_ShouldReturn200AndInactiveStatus()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        _factory.SenderMock.Setup(
            sender => sender.Send(
                It.Is<DeactivateCustomerCommand>(
                    command => command.CustomerId == customerId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customerId);

        // Act
        var response = await _client.PatchAsync($"/api/Customers/{customerId}/deactivate", content: null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<CustomerStatusResponse>();

        Assert.NotNull(body);
        Assert.Equal(customerId, body.Id);
        Assert.Equal("Inactive", body.Status);
    }


    [Fact]
    public async Task GetById_WhenCustomerExists_ShouldReturn200AndCustomer()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        var customer = new CustomerDto(
            customerId,
            "Ali",
            "Khan",
            "ali.khan@example.com",
            "+92",
            "3001234567",
            "Main Street",
            "Islamabad",
            "ICT",
            "Pakistan",
            "44000",
            CustomerStatus.Active,
            DateTime.UtcNow);

        _factory.SenderMock
            .Setup(sender => sender.Send(
                It.Is<GetCustomerByIdQuery>(
                    query => query.CustomerId == customerId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        // Act
        var response = await _client.GetAsync(
            $"/api/Customers/{customerId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result =
            await response.Content.ReadFromJsonAsync<CustomerDto>();

        Assert.NotNull(result);
        Assert.Equal(customerId, result.Id);
        Assert.Equal("Ali", result.FirstName);
        Assert.Equal("ali.khan@example.com", result.Email);
        Assert.Equal(CustomerStatus.Active, result.Status);
    }

    [Fact]
    public async Task GetAll_ShouldReturnPaginatedCustomers()
    {
        // Arrange
        var customer1 = new CustomerDto(
            Guid.NewGuid(),
            "Ali",
            "Khan",
            "ali.khan@example.com",
            "+92",
            "3001234567",
            "Main Street",
            "Islamabad",
            "ICT",
            "Pakistan",
            "44000",
            CustomerStatus.Active,
            DateTime.UtcNow);

        var customer2 = new CustomerDto(
            Guid.NewGuid(),
            "Sara",
            "Ahmed",
            "sara.ahmed@example.com",
            "+92",
            "3011234567",
            "Park Road",
            "Rawalpindi",
            "Punjab",
            "Pakistan",
            "46000",
            CustomerStatus.Active,
            DateTime.UtcNow);

        var result = new PaginatedResult<CustomerDto>(
            new[] { customer1, customer2 },
            PageNumber: 1,
            PageSize: 20,
            TotalCount: 2);

        _factory.SenderMock
            .Setup(sender => sender.Send(
                It.Is<GetCustomersQuery>(
                    query => query.Pagination.PageNumber == 1 &&
                             query.Pagination.PageSize == 20),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _client.GetAsync(
            "/api/Customers?pageNumber=1&pageSize=20");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body =
            await response.Content.ReadFromJsonAsync<
                PaginatedResult<CustomerDto>>();

        Assert.NotNull(body);
        Assert.Equal(1, body.PageNumber);
        Assert.Equal(20, body.PageSize);
        Assert.Equal(2, body.TotalCount);
        Assert.Equal(2, body.Items.Count);
        Assert.Equal(1, body.TotalPages);
        Assert.False(body.HasNextPage);
    }


    private sealed record CustomerStatusResponse(
        Guid Id,
        string Status);
}

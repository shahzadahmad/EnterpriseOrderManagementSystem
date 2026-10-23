
using EnterpriseOrderManagementSystem.Domain.Enums;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace EnterpriseOrderManagementSystem.Api.Tests;

[CollectionDefinition("Persistence API")]
public sealed class PersistenceApiCollection
    : ICollectionFixture<PersistenceApiTestFixture>
{
}

[Collection("Persistence API")]
public sealed class PersistenceApiTests
{
    private readonly HttpClient _client;

    public PersistenceApiTests(PersistenceApiTestFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task Customer_CreateAndRetrieve_ShouldPersistInDatabase()
    {
        // Arrange
        var email = $"customer-{Guid.NewGuid():N}@example.com";

        var request = new
        {
            FirstName = "Integration",
            LastName = "Customer",
            Email = email,
            PhoneCountryCode = "+92",
            PhoneNationalNumber = "3001234567",
            Street = "Integration Street",
            City = "Islamabad",
            State = "ICT",
            Country = "Pakistan",
            PostalCode = "44000"
        };

        // Act: create through the real API and application pipeline.
        var createResponse = await _client.PostAsJsonAsync(
            "/api/Customers",
            request);

        // Assert creation.
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        using var createJson = JsonDocument.Parse(
            await createResponse.Content.ReadAsStringAsync());

        var customerId = createJson.RootElement
            .GetProperty("id")
            .GetGuid();

        Assert.NotEqual(Guid.Empty, customerId);

        // Act: retrieve through the real API and repository.

        var getResponse = await _client.GetAsync(
            $"/api/Customers/{customerId}");

        // Assert persisted data.
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        using var getJson = JsonDocument.Parse(
            await getResponse.Content.ReadAsStringAsync());

        var customer = getJson.RootElement;

        Assert.Equal(
            customerId,
            customer.GetProperty("id").GetGuid());

        Assert.Equal(
            email,
            customer.GetProperty("email").GetString());

        Assert.Equal(
            "Integration",
            customer.GetProperty("firstName").GetString());
    }

    [Fact]
    public async Task Customer_StatusTransitions_ShouldPersistAcrossRequests()
    {
        var request = new
        {
            FirstName = "Status",
            LastName = "Integration",
            Email = $"status-{Guid.NewGuid():N}@example.com",
            PhoneCountryCode = "+92",
            PhoneNationalNumber = "3001234567",
            Street = "Integration Street",
            City = "Islamabad",
            State = "ICT",
            Country = "Pakistan",
            PostalCode = "44000"
        };

        var createResponse = await _client.PostAsJsonAsync(
            "/api/Customers", request);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        using var created = JsonDocument.Parse(
            await createResponse.Content.ReadAsStringAsync());

        var id = created.RootElement.GetProperty("id").GetGuid();

        var deactivateResponse = await _client.PatchAsync(
            $"/api/Customers/{id}/deactivate", null);

        Assert.Equal(HttpStatusCode.OK, deactivateResponse.StatusCode);

        var afterDeactivate = await _client.GetFromJsonAsync<JsonElement>(
            $"/api/Customers/{id}");

        var inactiveStatus = afterDeactivate.GetProperty("status").ToString();

        var activateResponse = await _client.PatchAsync(
            $"/api/Customers/{id}/activate", null);

        Assert.Equal(HttpStatusCode.OK, activateResponse.StatusCode);

        var afterActivate = await _client.GetFromJsonAsync<JsonElement>(
            $"/api/Customers/{id}");

        var activeStatus = afterActivate.GetProperty("status").ToString();

        Assert.NotEqual(inactiveStatus, activeStatus);
    }

    [Fact]
    public async Task Product_CreateAndRetrieve_ShouldPersistInDatabase()
    {
        // Arrange
        var sku = $"IT-{Guid.NewGuid():N}";

        var request = new
        {
            SKU = sku,
            Name = "Integration Test Product",
            Description = "Persisted through the API",
            Price = 125.50m,
            Currency = "USD"
        };

        // Act: create through the real API and application pipeline.
        var createResponse = await _client.PostAsJsonAsync(
            "/api/Products",
            request);

        // Assert creation.
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        using var createJson = JsonDocument.Parse(
            await createResponse.Content.ReadAsStringAsync());

        var productId = createJson.RootElement
            .GetProperty("id")
            .GetGuid();

        Assert.NotEqual(Guid.Empty, productId);

        // Act: retrieve through the real API and repository.
        var getResponse = await _client.GetAsync(
            $"/api/Products/{productId}");

        // Assert persisted data.
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        using var getJson = JsonDocument.Parse(
            await getResponse.Content.ReadAsStringAsync());

        var product = getJson.RootElement;

        Assert.Equal(
            productId,
            product.GetProperty("id").GetGuid());

        Assert.Equal(
            sku,
            product.GetProperty("sku").GetString());

        Assert.Equal(
            "Integration Test Product",
            product.GetProperty("name").GetString());
    }

    [Fact]
    public async Task Product_Update_ShouldPersistAcrossRequests()
    {
        var request = new
        {
            SKU = $"UPDATE-{Guid.NewGuid():N}",
            Name = "Original Integration Product",
            Description = "Original description",
            Price = 100.00m,
            Currency = "USD"
        };

        var createResponse = await _client.PostAsJsonAsync(
            "/api/Products", request);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        using var created = JsonDocument.Parse(
            await createResponse.Content.ReadAsStringAsync());

        var id = created.RootElement.GetProperty("id").GetGuid();

        var updateRequest = new
        {
            Name = "Updated Integration Product",
            Description = "Updated description"
        };

        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/Products/{id}", updateRequest);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var product = await _client.GetFromJsonAsync<JsonElement>(
            $"/api/Products/{id}");

        Assert.Equal(
            "Updated Integration Product",
            product.GetProperty("name").GetString());

        Assert.Equal(
            "Updated description",
            product.GetProperty("description").GetString());
    }

    [Fact]
    public async Task Order_CreateAndRetrieve_ShouldPersistInDatabase()
    {
        // Arrange: create a real customer first.
        var customerRequest = new
        {
            FirstName = "Order",
            LastName = "Integration",
            Email = $"order-{Guid.NewGuid():N}@example.com",
            PhoneCountryCode = "+92",
            PhoneNationalNumber = "3001234567",
            Street = "Integration Street",
            City = "Islamabad",
            State = "ICT",
            Country = "Pakistan",
            PostalCode = "44000"
        };

        var customerResponse = await _client.PostAsJsonAsync(
            "/api/Customers",
            customerRequest);

        Assert.Equal(
            HttpStatusCode.Created,
            customerResponse.StatusCode);

        using var customerJson = JsonDocument.Parse(
            await customerResponse.Content.ReadAsStringAsync());

        var customerId = customerJson.RootElement
            .GetProperty("id")
            .GetGuid();

        Assert.NotEqual(Guid.Empty, customerId);

        // Act: create an order for the persisted customer.
        var orderRequest = new
        {
            CustomerId = customerId
        };

        var createResponse = await _client.PostAsJsonAsync(
            "/api/Orders",
            orderRequest);

        // Assert: the API created the order.
        Assert.Equal(
            HttpStatusCode.Created,
            createResponse.StatusCode);

        using var createdOrderJson = JsonDocument.Parse(
            await createResponse.Content.ReadAsStringAsync());

        var orderId = createdOrderJson.RootElement
            .GetProperty("id")
            .GetGuid();

        Assert.NotEqual(Guid.Empty, orderId);

        // Act: retrieve the order through a separate API request.
        var getResponse = await _client.GetAsync(
            $"/api/Orders/{orderId}");

        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode);

        using var retrievedOrderJson = JsonDocument.Parse(
            await getResponse.Content.ReadAsStringAsync());

        var order = retrievedOrderJson.RootElement;

        // Assert: verify the order was persisted correctly.
        Assert.Equal(
            orderId,
            order.GetProperty("id").GetGuid());

        Assert.Equal(
            customerId,
            order.GetProperty("customerId").GetGuid());

        Assert.False(
            string.IsNullOrWhiteSpace(
                order.GetProperty("orderNumber").GetString()));

        Assert.True(
            order.GetProperty("createdOnUtc").GetDateTime()
                != default);

        Assert.Equal(
            0m,
            order.GetProperty("totalAmount").GetDecimal());

        Assert.Equal(
            "USD",
            order.GetProperty("currency").GetString());

        Assert.Empty(
            order.GetProperty("items").EnumerateArray());
    }

    [Fact]
    public async Task Order_Cancel_ShouldPersistReasonAndTimestamp()
    {
        // Arrange: create a customer.
        var customerRequest = new
        {
            FirstName = "Cancellation",
            LastName = "Integration",
            Email = $"cancel-{Guid.NewGuid():N}@example.com",
            PhoneCountryCode = "+92",
            PhoneNationalNumber = "3001234567",
            Street = "Integration Street",
            City = "Islamabad",
            State = "ICT",
            Country = "Pakistan",
            PostalCode = "44000"
        };

        var customerResponse = await _client.PostAsJsonAsync(
            "/api/Customers",
            customerRequest);

        Assert.Equal(
            HttpStatusCode.Created,
            customerResponse.StatusCode);

        using var customerJson = JsonDocument.Parse(
            await customerResponse.Content.ReadAsStringAsync());

        var customerId = customerJson.RootElement
            .GetProperty("id")
            .GetGuid();

        // Arrange: create an order for that customer.
        var createOrderResponse = await _client.PostAsJsonAsync(
            "/api/Orders",
            new { CustomerId = customerId });

        Assert.Equal(
            HttpStatusCode.Created,
            createOrderResponse.StatusCode);

        using var orderJson = JsonDocument.Parse(
            await createOrderResponse.Content.ReadAsStringAsync());

        var orderId = orderJson.RootElement
            .GetProperty("id")
            .GetGuid();

        const string reason = "Customer requested cancellation";

        // Act: cancel the order.
        var cancelResponse = await _client.PatchAsJsonAsync(
            $"/api/Orders/{orderId}/cancel",
            new { Reason = reason });

        Assert.Equal(HttpStatusCode.OK, cancelResponse.StatusCode);

        // Act: retrieve the order in a separate API request.
        var getResponse = await _client.GetAsync(
            $"/api/Orders/{orderId}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        using var retrievedJson = JsonDocument.Parse(
            await getResponse.Content.ReadAsStringAsync());

        var order = retrievedJson.RootElement;

        // Assert: cancellation details survived the database round trip.
        Assert.Equal(
            orderId,
            order.GetProperty("id").GetGuid());

        Assert.Equal(
            (int)OrderStatus.Cancelled,
            order.GetProperty("status").GetInt32());

        Assert.Equal(
            reason,
            order.GetProperty("cancellationReason").GetString());

        Assert.True(
            order.GetProperty("cancelledOnUtc").ValueKind
                != JsonValueKind.Null);

        var cancelledOnUtc = order
            .GetProperty("cancelledOnUtc")
            .GetDateTime();

        Assert.NotEqual(default, cancelledOnUtc);
    }

    [Fact]
    public async Task Order_LifecycleTransitions_ShouldPersistAcrossRequests()
    {
        // Arrange: create a customer.
        var customerResponse = await _client.PostAsJsonAsync(
            "/api/Customers",
            new
            {
                FirstName = "Lifecycle",
                LastName = "Integration",
                Email = $"lifecycle-{Guid.NewGuid():N}@example.com",
                PhoneCountryCode = "+92",
                PhoneNationalNumber = "3001234567",
                Street = "Integration Street",
                City = "Islamabad",
                State = "ICT",
                Country = "Pakistan",
                PostalCode = "44000"
            });

        Assert.Equal(
            HttpStatusCode.Created,
            customerResponse.StatusCode);

        using var customerJson = JsonDocument.Parse(
            await customerResponse.Content.ReadAsStringAsync());

        var customerId = customerJson.RootElement
            .GetProperty("id")
            .GetGuid();

        // Arrange: create an active product.
        var productResponse = await _client.PostAsJsonAsync(
            "/api/Products",
            new
            {
                SKU = $"LIFECYCLE-{Guid.NewGuid():N}",
                Name = "Lifecycle Test Product",
                Description = "Product for order lifecycle integration testing",
                Price = 125.50m,
                Currency = "USD"
            });

        Assert.Equal(
            HttpStatusCode.Created,
            productResponse.StatusCode);

        using var productJson = JsonDocument.Parse(
            await productResponse.Content.ReadAsStringAsync());

        var productId = productJson.RootElement
            .GetProperty("id")
            .GetGuid();

        // Arrange: create an order.
        var createOrderResponse = await _client.PostAsJsonAsync(
            "/api/Orders",
            new { CustomerId = customerId });

        Assert.Equal(
            HttpStatusCode.Created,
            createOrderResponse.StatusCode);

        using var orderJson = JsonDocument.Parse(
            await createOrderResponse.Content.ReadAsStringAsync());

        var orderId = orderJson.RootElement
            .GetProperty("id")
            .GetGuid();

        // Arrange: add an item so the order can be confirmed.
        var addItemResponse = await _client.PostAsJsonAsync(
            $"/api/Orders/{orderId}/items",
            new
            {
                ProductId = productId,
                Quantity = 2
            });

        Assert.Equal(
            HttpStatusCode.OK,
            addItemResponse.StatusCode);

        // Act: confirm the order.
        var confirmResponse = await _client.PatchAsync(
            $"/api/Orders/{orderId}/confirm",
            content: null);

        Assert.Equal(
            HttpStatusCode.OK,
            confirmResponse.StatusCode);

        // Assert: confirmed status is persisted.
        var confirmedOrder = await _client.GetFromJsonAsync<JsonElement>(
            $"/api/Orders/{orderId}");

        Assert.Equal(
            (int)OrderStatus.Confirmed,
            confirmedOrder.GetProperty("status").GetInt32());

        // Act: ship the order.
        var shipResponse = await _client.PatchAsync(
            $"/api/Orders/{orderId}/ship",
            content: null);

        Assert.Equal(
            HttpStatusCode.OK,
            shipResponse.StatusCode);

        // Assert: shipped status is persisted.
        var shippedOrder = await _client.GetFromJsonAsync<JsonElement>(
            $"/api/Orders/{orderId}");

        Assert.Equal(
            (int)OrderStatus.Shipped,
            shippedOrder.GetProperty("status").GetInt32());

        // Act: deliver the order.
        var deliverResponse = await _client.PatchAsync(
            $"/api/Orders/{orderId}/deliver",
            content: null);

        Assert.Equal(
            HttpStatusCode.OK,
            deliverResponse.StatusCode);

        // Assert: delivered status is persisted.
        var deliveredOrder = await _client.GetFromJsonAsync<JsonElement>(
            $"/api/Orders/{orderId}");

        Assert.Equal(
            (int)OrderStatus.Delivered,
            deliveredOrder.GetProperty("status").GetInt32());

        Assert.Equal(
            orderId,
            deliveredOrder.GetProperty("id").GetGuid());

        Assert.Equal(
            customerId,
            deliveredOrder.GetProperty("customerId").GetGuid());

        Assert.Equal(
            251.00m,
            deliveredOrder.GetProperty("totalAmount").GetDecimal());

        Assert.Single(
            deliveredOrder.GetProperty("items").EnumerateArray());
    }

}

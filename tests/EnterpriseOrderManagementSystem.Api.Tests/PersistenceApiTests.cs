
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

}

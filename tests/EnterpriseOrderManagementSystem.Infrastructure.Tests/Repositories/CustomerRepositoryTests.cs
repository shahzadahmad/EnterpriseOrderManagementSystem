using EnterpriseOrderManagementSystem.Domain.Aggregates.CustomerAggregate;
using EnterpriseOrderManagementSystem.Domain.Common.ValueObjects;
using EnterpriseOrderManagementSystem.Infrastructure.Persistence.Repositories;
using EnterpriseOrderManagementSystem.Infrastructure.Tests.Database;
using Xunit;

namespace EnterpriseOrderManagementSystem.Infrastructure.Tests.Repositories;

[Collection("SQL Server")]
public sealed class CustomerRepositoryTests
{
    private readonly SqlServerFixture _fixture;

    public CustomerRepositoryTests(
        SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    #region Add And Get By Id

    [Fact]
    public async Task AddAsync_ThenGetByIdAsync_ShouldPersistCustomer()
    {
        await using var context =
            _fixture.CreateDbContext();

        var repository =
            new CustomerRepository(context);

        var customer =
            Customer.Create(
                "John",
                "Doe",
                Email.Create("john.doe@test.com"),
                PhoneNumber.Create(
                    "+1",
                    "5551234567"),
                Address.Create(
                    "123 Main Street",
                    "Dallas",
                    "Texas",
                    "USA",
                    "75001"));

        await repository.AddAsync(customer);

        await context.SaveChangesAsync();

        await using var verificationContext =
            _fixture.CreateDbContext();

        var verificationRepository =
            new CustomerRepository(
                verificationContext);

        var result =
            await verificationRepository.GetByIdAsync(
                customer.Id);

        Assert.NotNull(result);

        Assert.Equal(
            customer.Id,
            result.Id);

        Assert.Equal(
            "John",
            result.FirstName);

        Assert.Equal(
            "Doe",
            result.LastName);

        Assert.Equal(
            "john.doe@test.com",
            result.Email.Value);

        Assert.Equal(
            "+1",
            result.PhoneNumber.CountryCode);

        Assert.Equal(
            "5551234567",
            result.PhoneNumber.NationalNumber);
    }

    #endregion

    #region Get By Email

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnCustomer()
    {
        await using var context =
            _fixture.CreateDbContext();

        var repository =
            new CustomerRepository(context);

        var customer =
            Customer.Create(
                "Jane",
                "Smith",
                Email.Create("jane.smith@test.com"),
                PhoneNumber.Create(
                    "+1",
                    "5559876543"),
                Address.Create(
                    "456 Oak Street",
                    "Austin",
                    "Texas",
                    "USA",
                    "78701"));

        await repository.AddAsync(customer);

        await context.SaveChangesAsync();

        var result =
            await repository.GetByEmailAsync(
                "jane.smith@test.com");

        Assert.NotNull(result);

        Assert.Equal(
            customer.Id,
            result.Id);
    }

    #endregion
}
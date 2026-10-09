using EnterpriseOrderManagementSystem.Domain.Aggregates.CustomerAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.ProductAggregate;
using EnterpriseOrderManagementSystem.Domain.Common.ValueObjects;
using EnterpriseOrderManagementSystem.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseOrderManagementSystem.Infrastructure.Persistence.Seeding;

public sealed class DatabaseSeeder
{
    private readonly AppDbContext _context;

    public DatabaseSeeder(
        AppDbContext context)
    {
        _context = context;
    }

    #region Seed Database

    public async Task SeedAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.Database.MigrateAsync(
            cancellationToken);

        await SeedProductsAsync(
            cancellationToken);

        await SeedCustomersAsync(
            cancellationToken);

        await _context.SaveChangesAsync(
            cancellationToken);
    }

    #endregion

    #region Seed Products

    private async Task SeedProductsAsync(
        CancellationToken cancellationToken)
    {
        if (await _context.Products.AnyAsync(
                cancellationToken))
        {
            return;
        }

        var products = new[]
        {
            Product.Create(
                "SKU-001",
                "Laptop",
                "Development laptop",
                Money.Create(1500m, "USD")),
            

            Product.Create(
                "SKU-002",
                "Monitor",
                "27 inch development monitor",
                Money.Create(400m, "USD")),

            Product.Create(
                "SKU-003",
                "Keyboard",
                "Mechanical keyboard",
                Money.Create(120m, "USD"))
        };

        await _context.Products.AddRangeAsync(
            products,
            cancellationToken);
    }

    #endregion

    #region Seed Customers

    private async Task SeedCustomersAsync(
        CancellationToken cancellationToken)
    {
        if (await _context.Customers.AnyAsync(
                cancellationToken))
        {
            return;
        }

        var customer = Customer.Create(
            "John",
            "Doe",
            Email.Create("john.doe@example.com"),
            PhoneNumber.Create("+1","5551234567"),
            Address.Create(
                "123 Main Street",
                "Dallas",
                "Texas",
                "USA",
                "75001"));

        await _context.Customers.AddAsync(
            customer,
            cancellationToken);
    }

    #endregion
}
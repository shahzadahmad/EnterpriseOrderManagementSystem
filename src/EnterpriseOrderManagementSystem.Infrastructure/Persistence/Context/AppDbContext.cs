using EnterpriseOrderManagementSystem.Domain.Aggregates.CustomerAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.PaymentAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.ProductAggregate;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseOrderManagementSystem.Infrastructure.Persistence.Context;

public sealed class AppDbContext : DbContext
{
    #region Constructor

    public AppDbContext(
        DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    #endregion

    #region DbSets

    public DbSet<Customer> Customers =>
        Set<Customer>();

    public DbSet<Product> Products =>
        Set<Product>();

    public DbSet<Order> Orders =>
        Set<Order>();

    public DbSet<Inventory> Inventories =>
        Set<Inventory>();

    public DbSet<Payment> Payments =>
        Set<Payment>();

    #endregion

    #region Model Configuration

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);
    }

    #endregion
}
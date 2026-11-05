
using EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseOrderManagementSystem.Infrastructure.Persistence.Configurations;

public sealed class InventoryConfiguration
    : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> builder)
    {
        builder.ToTable("Inventories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.Property(x => x.WarehouseId)
            .IsRequired();

        builder.Property(x => x.AvailableQuantity)
            .IsRequired();

        builder.Property(x => x.ReservedQuantity)
            .IsRequired();

        builder.Property(x => x.ReorderLevel)
            .IsRequired();

        builder.Property(x => x.MaximumQuantity)
            .IsRequired();

        builder.Property(x => x.IsDiscontinued)
            .IsRequired();

        builder.Property(x => x.LastStockMovementUtc)
            .IsRequired();

        builder.Ignore(x => x.Status);

        builder.HasIndex(x => new
        {
            x.ProductId,
            x.WarehouseId
        })
        .IsUnique();

        // Inventory owns its transaction history.
        builder.HasMany(x => x.Transactions)
            .WithOne(x => x.Inventory)
            .HasForeignKey("InventoryId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // Explicitly use the private collection rather than
        // relying on EF Core to access the read-only property.
        builder.Navigation(x => x.Transactions)
            .HasField("_transactions")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

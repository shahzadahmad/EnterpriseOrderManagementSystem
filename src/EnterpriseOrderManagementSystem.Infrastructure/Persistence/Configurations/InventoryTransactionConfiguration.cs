
using EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseOrderManagementSystem.Infrastructure.Persistence.Configurations;

public sealed class InventoryTransactionConfiguration
    : IEntityTypeConfiguration<InventoryTransaction>
{
    public void Configure(
        EntityTypeBuilder<InventoryTransaction> builder)
    {
        builder.ToTable("InventoryTransaction");

        builder.HasKey(x => x.Id);

        // InventoryTransaction.Create() generates the GUID.
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.MovementType)
            .IsRequired();

        builder.Property(x => x.Quantity)
            .IsRequired();

        builder.Property(x => x.AvailableQuantityBefore)
            .IsRequired();

        builder.Property(x => x.AvailableQuantityAfter)
            .IsRequired();

        builder.Property(x => x.ReservedQuantityBefore)
            .IsRequired();

        builder.Property(x => x.ReservedQuantityAfter)
            .IsRequired();

        builder.Property(x => x.Reference)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.OccurredOnUtc)
            .IsRequired();

        // The Inventory relationship and shadow FK are configured
        // in InventoryConfiguration.
    }
}

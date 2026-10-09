using EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseOrderManagementSystem.Infrastructure.Persistence.Configurations;

public sealed class InventoryConfiguration
    : IEntityTypeConfiguration<Inventory>
{
    public void Configure(
        EntityTypeBuilder<Inventory> builder)
    {
        #region Table

        builder.ToTable("Inventories");

        #endregion

        #region Primary Key

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        #endregion

        #region Product and Warehouse

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.Property(x => x.WarehouseId)
            .IsRequired();

        #endregion

        #region Quantity Information

        builder.Property(x => x.AvailableQuantity)
            .IsRequired();

        builder.Property(x => x.ReservedQuantity)
            .IsRequired();

        builder.Property(x => x.ReorderLevel)
            .IsRequired();

        builder.Property(x => x.MaximumQuantity)
            .IsRequired();

        #endregion

        #region Status Information

        builder.Property(x => x.IsDiscontinued)
            .IsRequired();

        builder.Property(x => x.LastStockMovementUtc)
            .IsRequired();

        builder.Ignore(x => x.Status);

        #endregion

        #region Product and Warehouse Uniqueness

        builder.HasIndex(x => new
        {
            x.ProductId,
            x.WarehouseId
        })
        .IsUnique();

        #endregion

        #region Inventory Transactions

        builder.HasMany(x => x.Transactions)
            .WithOne(x => x.Inventory)
            .HasForeignKey("InventoryId")
            .OnDelete(DeleteBehavior.Cascade);

        #endregion
    }
}
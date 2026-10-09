using EnterpriseOrderManagementSystem.Domain.Aggregates.ProductAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseOrderManagementSystem.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration
    : IEntityTypeConfiguration<Product>
{
    public void Configure(
        EntityTypeBuilder<Product> builder)
    {
        #region Table

        builder.ToTable("Products");

        #endregion

        #region Primary Key

        builder.HasKey(x => x.Id);

        #endregion

        #region Identity

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        #endregion

        #region Basic Properties

        builder.Property(x => x.SKU)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(x => x.SKU)
            .IsUnique();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.CreatedOnUtc)
            .IsRequired();

        #endregion

        #region Price Value Object

        builder.OwnsOne(
            x => x.Price,
            money =>
            {
                money.Property(x => x.Amount)
                    .HasColumnName("PriceAmount")
                    .HasPrecision(18, 2)
                    .IsRequired();

                money.Property(x => x.Currency)
                    .HasColumnName("PriceCurrency")
                    .HasMaxLength(3)
                    .IsRequired();
            });

        #endregion
    }
}
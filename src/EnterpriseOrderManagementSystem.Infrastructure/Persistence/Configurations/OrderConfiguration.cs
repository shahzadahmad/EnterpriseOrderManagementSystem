using EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseOrderManagementSystem.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration
    : IEntityTypeConfiguration<Order>
{
    public void Configure(
        EntityTypeBuilder<Order> builder)
    {
        #region Table

        builder.ToTable("Orders");

        #endregion

        #region Primary Key

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        #endregion

        #region Order Information

        builder.Property(x => x.CustomerId)
            .IsRequired();

        builder.Property(x => x.OrderNumber)
            .HasColumnName("OrderNumber")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.OrderNumber)
            .IsUnique();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.CreatedOnUtc)
            .HasColumnName("CreatedOnUtc")
            .IsRequired();

        #endregion

        #region Total Amount Value Object

        builder.OwnsOne(
            x => x.TotalAmount,
            money =>
            {
                money.Property(x => x.Amount)
                    .HasColumnName("TotalAmount")
                    .HasPrecision(18, 2)
                    .IsRequired();

                money.Property(x => x.Currency)
                    .HasColumnName("Currency")
                    .HasMaxLength(3)
                    .IsRequired();
            });

        #endregion

        #region Order Item Configuration

        builder.Navigation(x => x.Items)
            .UsePropertyAccessMode(
                PropertyAccessMode.Field);

        builder.OwnsMany(
            x => x.Items,
            item =>
            {
                item.ToTable("OrderItems");

                item.HasKey("Id");

                item.Property<Guid>("Id")
                    .ValueGeneratedNever();

                item.Property(x => x.ProductId)
                    .IsRequired();

                item.Property(x => x.ProductName)
                    .HasMaxLength(200)
                    .IsRequired();

                item.Property(x => x.Quantity)
                    .IsRequired();

                #region Unit Price Value Object

                item.OwnsOne(
                    x => x.UnitPrice,
                    money =>
                    {
                        money.Property(x => x.Amount)
                            .HasColumnName("UnitPriceAmount")
                            .HasPrecision(18, 2)
                            .IsRequired();

                        money.Property(x => x.Currency)
                            .HasColumnName("UnitPriceCurrency")
                            .HasMaxLength(3)
                            .IsRequired();
                    });

                #endregion

                #region Line Total Value Object

                item.OwnsOne(
                    x => x.LineTotal,
                    money =>
                    {
                        money.Property(x => x.Amount)
                            .HasColumnName("LineTotalAmount")
                            .HasPrecision(18, 2)
                            .IsRequired();

                        money.Property(x => x.Currency)
                            .HasColumnName("LineTotalCurrency")
                            .HasMaxLength(3)
                            .IsRequired();
                    });

                #endregion
            });

        #endregion
    }
}
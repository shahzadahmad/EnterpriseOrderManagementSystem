using EnterpriseOrderManagementSystem.Domain.Aggregates.PaymentAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseOrderManagementSystem.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration
    : IEntityTypeConfiguration<Payment>
{
    public void Configure(
        EntityTypeBuilder<Payment> builder)
    {
        #region Table

        builder.ToTable("Payments");

        #endregion

        #region Primary Key

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        #endregion

        #region Order Relationship

        builder.Property(x => x.OrderId)
            .IsRequired();

        builder.HasIndex(x => x.OrderId)
            .IsUnique();

        #endregion

        #region Payment Information

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(x => x.PaymentMethod)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.PaymentProvider)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        #endregion

        #region Provider Information

        builder.Property(x => x.ProviderReference)
            .HasMaxLength(200);

        builder.Property(x => x.AuthorizationCode)
            .HasMaxLength(100);

        #endregion

        #region Failure and Cancellation Information

        builder.Property(x => x.FailureCode)
            .HasMaxLength(100);

        builder.Property(x => x.FailureReason)
            .HasMaxLength(500);

        builder.Property(x => x.CancellationReason)
            .HasMaxLength(500);

        #endregion

        #region Refund Information

        builder.Property(x => x.TotalRefundedAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        #endregion

        #region Lifecycle Timestamps

        builder.Property(x => x.CreatedOnUtc)
            .IsRequired();

        builder.Property(x => x.AuthorizedOnUtc);

        builder.Property(x => x.CapturedOnUtc);

        builder.Property(x => x.SettledOnUtc);

        builder.Property(x => x.RefundedOnUtc);

        builder.Property(x => x.CancelledOnUtc);

        builder.Property(x => x.FailedOnUtc);

        #endregion

        #region Payment Transactions

        builder.HasMany(x => x.Transactions)
            .WithOne()
            .HasForeignKey("PaymentId")
            .OnDelete(DeleteBehavior.Cascade);

        #endregion
    }
}
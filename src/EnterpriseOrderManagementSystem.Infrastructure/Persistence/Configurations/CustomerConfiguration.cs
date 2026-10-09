using EnterpriseOrderManagementSystem.Domain.Aggregates.CustomerAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseOrderManagementSystem.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration
    : IEntityTypeConfiguration<Customer>
{
    public void Configure(
        EntityTypeBuilder<Customer> builder)
    {
        #region Table

        builder.ToTable("Customers");

        #endregion

        #region Primary Key

        builder.HasKey(x => x.Id);

        #endregion

        #region Identity

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        #endregion

        #region Basic Properties

        builder.Property(x => x.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.RegisteredOnUtc)
            .IsRequired();

        #endregion

        #region Email Value Object

        builder.OwnsOne(
            x => x.Email,
            email =>
            {
                email.Property(x => x.Value)
                    .HasColumnName("Email")
                    .HasMaxLength(320)
                    .IsRequired();

                email.HasIndex(x => x.Value)
                    .IsUnique();
            });

        #endregion

        #region Phone Number Value Object

        builder.OwnsOne(
            x => x.PhoneNumber,
            phone =>
            {
                phone.Property(x => x.CountryCode)
                    .HasColumnName("PhoneCountryCode")
                    .HasMaxLength(10)
                    .IsRequired();

                phone.Property(x => x.NationalNumber)
                    .HasColumnName("PhoneNationalNumber")
                    .HasMaxLength(20)
                    .IsRequired();
            });

        #endregion

        #region Address Value Object

        builder.OwnsOne(
            x => x.Address,
            address =>
            {
                address.Property(x => x.Street)
                    .HasColumnName("Street")
                    .HasMaxLength(200)
                    .IsRequired();

                address.Property(x => x.City)
                    .HasColumnName("City")
                    .HasMaxLength(100)
                    .IsRequired();

                address.Property(x => x.State)
                    .HasColumnName("State")
                    .HasMaxLength(100)
                    .IsRequired();

                address.Property(x => x.Country)
                    .HasColumnName("Country")
                    .HasMaxLength(100)
                    .IsRequired();

                address.Property(x => x.PostalCode)
                    .HasColumnName("PostalCode")
                    .HasMaxLength(20)
                    .IsRequired();
            });

        #endregion

    }
}
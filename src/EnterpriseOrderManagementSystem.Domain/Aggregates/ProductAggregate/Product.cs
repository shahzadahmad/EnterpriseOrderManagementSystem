using EnterpriseOrderManagementSystem.Domain.Aggregates.ProductAggregate.Events;
using EnterpriseOrderManagementSystem.Domain.Common.Base;
using EnterpriseOrderManagementSystem.Domain.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Common.ValueObjects;
using EnterpriseOrderManagementSystem.Domain.Enums;

namespace EnterpriseOrderManagementSystem.Domain.Aggregates.ProductAggregate;

/// <summary>
/// Represents a sellable product.
/// Aggregate Root.
/// </summary>
public sealed class Product : AggregateRoot<Guid>
{
    #region Constructors

    private Product()
    {
    }

    private Product(
        string sku,
        string name,
        string description,
        Money price)
    {
        Id = Guid.NewGuid();

        SKU = sku;
        Name = name;
        Description = description;
        Price = price;

        Status = ProductStatus.Active;

        CreatedOnUtc = DateTime.UtcNow;
    }

    #endregion

    #region Properties

    public string SKU { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public Money Price { get; private set; } = default!;

    public ProductStatus Status { get; private set; }

    public DateTime CreatedOnUtc { get; private set; }

    #endregion

    #region Factory Methods

    public static Product Create(
        string sku,
        string name,
        string description,
        Money price)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var product = new Product(
            sku.Trim(),
            name.Trim(),
            description.Trim(),
            price);

        product.AddDomainEvent(
            new ProductCreatedDomainEvent(product.Id));

        return product;
    }

    #endregion

    #region Update Methods

    public void UpdateDetails(
        string name,
        string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name.Trim();
        Description = description.Trim();
    }

    public void ChangePrice(Money newPrice)
    {
        if (Price == newPrice)
            return;

        var oldPrice = Price;

        Price = newPrice;

        AddDomainEvent(
            new ProductPriceChangedDomainEvent(
                Id,
                oldPrice,
                newPrice));
    }

    #endregion

    #region State Management

    public void Activate()
    {
        if (Status == ProductStatus.Active)
            throw new BusinessRuleViolationException(
                "Product is already active.");

        Status = ProductStatus.Active;

        AddDomainEvent(
            new ProductActivatedDomainEvent(Id));
    }

    public void Deactivate()
    {
        if (Status == ProductStatus.Inactive)
            throw new BusinessRuleViolationException(
                "Product is already inactive.");

        Status = ProductStatus.Inactive;

        AddDomainEvent(
            new ProductDeactivatedDomainEvent(Id));
    }

    public void Discontinue()
    {
        if (Status == ProductStatus.Discontinued)
            throw new BusinessRuleViolationException(
                "Product is already discontinued.");

        Status = ProductStatus.Discontinued;
    }

    #endregion
}
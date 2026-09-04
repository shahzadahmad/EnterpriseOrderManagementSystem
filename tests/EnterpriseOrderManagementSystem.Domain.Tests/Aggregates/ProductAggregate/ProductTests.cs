using EnterpriseOrderManagementSystem.Domain.Aggregates.ProductAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.ProductAggregate.Events;
using EnterpriseOrderManagementSystem.Domain.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Common.ValueObjects;
using EnterpriseOrderManagementSystem.Domain.Enums;
using FluentAssertions;

namespace EnterpriseOrderManagementSystem.Domain.Tests.Aggregates.ProductAggregate;

public sealed class ProductTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidData_CreatesProduct()
    {
        // Arrange
        var sku = "SKU-1001";
        var name = "Laptop";
        var description = "Business laptop";
        var price = CreateValidMoney();

        // Act
        var product = Product.Create(
            sku,
            name,
            description,
            price);

        // Assert
        product.Should().NotBeNull();

        product.Id
            .Should()
            .NotBe(Guid.Empty);

        product.SKU
            .Should()
            .Be(sku);

        product.Name
            .Should()
            .Be(name);

        product.Description
            .Should()
            .Be(description);

        product.Price
            .Should()
            .Be(price);

        product.Status
            .Should()
            .Be(ProductStatus.Active);

        product.CreatedOnUtc
            .Should()
            .BeCloseTo(
                DateTime.UtcNow,
                TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithValidData_GeneratesProductId()
    {
        // Arrange
        var price = CreateValidMoney();

        // Act
        var product = Product.Create(
            "SKU-1001",
            "Laptop",
            "Business laptop",
            price);

        // Assert
        product.Id
            .Should()
            .NotBe(Guid.Empty);
    }

    [Fact]
    public void Create_WithValidData_GeneratesUniqueProductIds()
    {
        // Arrange
        var price1 = CreateValidMoney();
        var price2 = CreateValidMoney();

        // Act
        var product1 = Product.Create(
            "SKU-1001",
            "Laptop",
            "Business laptop",
            price1);

        var product2 = Product.Create(
            "SKU-1002",
            "Desktop",
            "Business desktop",
            price2);

        // Assert
        product1.Id
            .Should()
            .NotBe(Guid.Empty);

        product2.Id
            .Should()
            .NotBe(Guid.Empty);

        product1.Id
            .Should()
            .NotBe(product2.Id);
    }

    #endregion

    #region SKU Validation Tests

    [Fact]
    public void Create_WithEmptySku_ThrowsArgumentException()
    {
        // Arrange
        var price = CreateValidMoney();

        // Act
        var action = () =>
            Product.Create(
                string.Empty,
                "Laptop",
                "Business laptop",
                price);

        // Assert
        action.Should()
            .Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithWhitespaceSku_ThrowsArgumentException()
    {
        // Arrange
        var price = CreateValidMoney();

        // Act
        var action = () =>
            Product.Create(
                "   ",
                "Laptop",
                "Business laptop",
                price);

        // Assert
        action.Should()
            .Throw<ArgumentException>();
    }

    #endregion

    #region Name Validation Tests

    [Fact]
    public void Create_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange
        var price = CreateValidMoney();

        // Act
        var action = () =>
            Product.Create(
                "SKU-1001",
                string.Empty,
                "Business laptop",
                price);

        // Assert
        action.Should()
            .Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithWhitespaceName_ThrowsArgumentException()
    {
        // Arrange
        var price = CreateValidMoney();

        // Act
        var action = () =>
            Product.Create(
                "SKU-1001",
                "   ",
                "Business laptop",
                price);

        // Assert
        action.Should()
            .Throw<ArgumentException>();
    }

    #endregion

    #region Trimming Tests

    [Fact]
    public void Create_WithWhitespaceAroundSku_TrimsSku()
    {
        // Arrange
        var price = CreateValidMoney();

        // Act
        var product = Product.Create(
            "  SKU-1001  ",
            "Laptop",
            "Business laptop",
            price);

        // Assert
        product.SKU
            .Should()
            .Be("SKU-1001");
    }

    [Fact]
    public void Create_WithWhitespaceAroundName_TrimsName()
    {
        // Arrange
        var price = CreateValidMoney();

        // Act
        var product = Product.Create(
            "SKU-1001",
            "  Laptop  ",
            "Business laptop",
            price);

        // Assert
        product.Name
            .Should()
            .Be("Laptop");
    }

    [Fact]
    public void Create_WithWhitespaceAroundDescription_TrimsDescription()
    {
        // Arrange
        var price = CreateValidMoney();

        // Act
        var product = Product.Create(
            "SKU-1001",
            "Laptop",
            "  Business laptop  ",
            price);

        // Assert
        product.Description
            .Should()
            .Be("Business laptop");
    }

    #endregion

    #region Initial State Tests

    [Fact]
    public void Create_WithValidData_SetsStatusToActive()
    {
        // Arrange
        var price = CreateValidMoney();

        // Act
        var product = Product.Create(
            "SKU-1001",
            "Laptop",
            "Business laptop",
            price);

        // Assert
        product.Status
            .Should()
            .Be(ProductStatus.Active);
    }

    [Fact]
    public void Create_WithValidData_SetsCreatedOnUtc()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var product = Product.Create(
            "SKU-1001",
            "Laptop",
            "Business laptop",
            CreateValidMoney());

        var afterCreation = DateTime.UtcNow;

        // Assert
        product.CreatedOnUtc
            .Should()
            .BeOnOrAfter(beforeCreation);

        product.CreatedOnUtc
            .Should()
            .BeOnOrBefore(afterCreation);
    }

    #endregion

    #region Domain Event Tests

    [Fact]
    public void Create_WithValidData_RaisesProductCreatedDomainEvent()
    {
        // Arrange
        var product = CreateValidProduct();

        // Assert
        product.DomainEvents
            .Should()
            .ContainSingle(eventItem =>
                eventItem is ProductCreatedDomainEvent);
    }

    [Fact]
    public void Create_WithValidData_ProductCreatedEventContainsProductId()
    {
        // Arrange
        var product = CreateValidProduct();

        // Act
        var domainEvent = product.DomainEvents
            .OfType<ProductCreatedDomainEvent>()
            .Single();

        // Assert
        domainEvent.ProductId
            .Should()
            .Be(product.Id);
    }

    #endregion

    #region Update Details Tests

    [Fact]
    public void UpdateDetails_WithValidData_UpdatesName()
    {
        // Arrange
        var product = CreateValidProduct();

        // Act
        product.UpdateDetails(
            "Updated Laptop",
            product.Description);

        // Assert
        product.Name
            .Should()
            .Be("Updated Laptop");
    }

    [Fact]
    public void UpdateDetails_WithValidData_UpdatesDescription()
    {
        // Arrange
        var product = CreateValidProduct();

        // Act
        product.UpdateDetails(
            product.Name,
            "Updated description");

        // Assert
        product.Description
            .Should()
            .Be("Updated description");
    }

    [Fact]
    public void UpdateDetails_WithValidData_TrimsName()
    {
        // Arrange
        var product = CreateValidProduct();

        // Act
        product.UpdateDetails(
            "  Updated Laptop  ",
            "Updated description");

        // Assert
        product.Name
            .Should()
            .Be("Updated Laptop");
    }

    [Fact]
    public void UpdateDetails_WithValidData_TrimsDescription()
    {
        // Arrange
        var product = CreateValidProduct();

        // Act
        product.UpdateDetails(
            "Updated Laptop",
            "  Updated description  ");

        // Assert
        product.Description
            .Should()
            .Be("Updated description");
    }

    [Fact]
    public void UpdateDetails_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange
        var product = CreateValidProduct();

        // Act
        var action = () =>
            product.UpdateDetails(
                string.Empty,
                "Updated description");

        // Assert
        action.Should()
            .Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateDetails_WithWhitespaceName_ThrowsArgumentException()
    {
        // Arrange
        var product = CreateValidProduct();

        // Act
        var action = () =>
            product.UpdateDetails(
                "   ",
                "Updated description");

        // Assert
        action.Should()
            .Throw<ArgumentException>();
    }

    #endregion

    #region Change Price Tests

    [Fact]
    public void ChangePrice_WithDifferentPrice_UpdatesPrice()
    {
        // Arrange
        var product = CreateValidProduct();
        var newPrice = CreateAnotherMoney();

        // Act
        product.ChangePrice(newPrice);

        // Assert
        product.Price
            .Should()
            .Be(newPrice);
    }

    [Fact]
    public void ChangePrice_WithDifferentPrice_RaisesProductPriceChangedDomainEvent()
    {
        // Arrange
        var product = CreateValidProduct();
        var newPrice = CreateAnotherMoney();

        // Act
        product.ChangePrice(newPrice);

        // Assert
        product.DomainEvents
            .Should()
            .ContainSingle(eventItem =>
                eventItem is ProductPriceChangedDomainEvent);
    }

    [Fact]
    public void ChangePrice_WithDifferentPrice_EventContainsProductId()
    {
        // Arrange
        var product = CreateValidProduct();
        var newPrice = CreateAnotherMoney();

        // Act
        product.ChangePrice(newPrice);

        // Assert
        var domainEvent = product.DomainEvents
            .OfType<ProductPriceChangedDomainEvent>()
            .Single();

        domainEvent.ProductId
            .Should()
            .Be(product.Id);
    }

    [Fact]
    public void ChangePrice_WithSamePrice_DoesNotChangePrice()
    {
        // Arrange
        var product = CreateValidProduct();
        var originalPrice = product.Price;

        // Act
        product.ChangePrice(originalPrice);

        // Assert
        product.Price
            .Should()
            .Be(originalPrice);
    }

    [Fact]
    public void ChangePrice_WithSamePrice_DoesNotRaisePriceChangedEvent()
    {
        // Arrange
        var product = CreateValidProduct();

        var initialPriceChangedEvents =
            product.DomainEvents
                .OfType<ProductPriceChangedDomainEvent>()
                .Count();

        // Act
        product.ChangePrice(product.Price);

        // Assert
        product.DomainEvents
            .OfType<ProductPriceChangedDomainEvent>()
            .Count()
            .Should()
            .Be(initialPriceChangedEvents);
    }

    #endregion

    #region Activate Tests

    [Fact]
    public void Activate_WhenProductIsInactive_ChangesStatusToActive()
    {
        // Arrange
        var product = CreateValidProduct();

        product.Deactivate();

        // Act
        product.Activate();

        // Assert
        product.Status
            .Should()
            .Be(ProductStatus.Active);
    }

    [Fact]
    public void Activate_WhenProductIsInactive_RaisesProductActivatedDomainEvent()
    {
        // Arrange
        var product = CreateValidProduct();

        product.Deactivate();

        // Act
        product.Activate();

        // Assert
        product.DomainEvents
            .Should()
            .Contain(eventItem =>
                eventItem is ProductActivatedDomainEvent);
    }

    [Fact]
    public void Activate_WhenProductIsAlreadyActive_ThrowsBusinessRuleViolationException()
    {
        // Arrange
        var product = CreateValidProduct();

        // Act
        var action = () =>
            product.Activate();

        // Assert
        action.Should()
            .Throw<BusinessRuleViolationException>()
            .WithMessage("Product is already active.");
    }

    #endregion

    #region Deactivate Tests

    [Fact]
    public void Deactivate_WhenProductIsActive_ChangesStatusToInactive()
    {
        // Arrange
        var product = CreateValidProduct();

        // Act
        product.Deactivate();

        // Assert
        product.Status
            .Should()
            .Be(ProductStatus.Inactive);
    }

    [Fact]
    public void Deactivate_WhenProductIsActive_RaisesProductDeactivatedDomainEvent()
    {
        // Arrange
        var product = CreateValidProduct();

        // Act
        product.Deactivate();

        // Assert
        product.DomainEvents
            .Should()
            .Contain(eventItem =>
                eventItem is ProductDeactivatedDomainEvent);
    }

    [Fact]
    public void Deactivate_WhenProductIsAlreadyInactive_ThrowsBusinessRuleViolationException()
    {
        // Arrange
        var product = CreateValidProduct();

        product.Deactivate();

        // Act
        var action = () =>
            product.Deactivate();

        // Assert
        action.Should()
            .Throw<BusinessRuleViolationException>()
            .WithMessage("Product is already inactive.");
    }

    #endregion

    #region Discontinue Tests

    [Fact]
    public void Discontinue_WhenProductIsActive_ChangesStatusToDiscontinued()
    {
        // Arrange
        var product = CreateValidProduct();

        // Act
        product.Discontinue();

        // Assert
        product.Status
            .Should()
            .Be(ProductStatus.Discontinued);
    }

    [Fact]
    public void Discontinue_WhenProductIsInactive_ChangesStatusToDiscontinued()
    {
        // Arrange
        var product = CreateValidProduct();

        product.Deactivate();

        // Act
        product.Discontinue();

        // Assert
        product.Status
            .Should()
            .Be(ProductStatus.Discontinued);
    }

    [Fact]
    public void Discontinue_WhenProductIsAlreadyDiscontinued_ThrowsBusinessRuleViolationException()
    {
        // Arrange
        var product = CreateValidProduct();

        product.Discontinue();

        // Act
        var action = () =>
            product.Discontinue();

        // Assert
        action.Should()
            .Throw<BusinessRuleViolationException>()
            .WithMessage("Product is already discontinued.");
    }

    #endregion

    #region Test Helpers

    private static Product CreateValidProduct()
    {
        return Product.Create(
            "SKU-1001",
            "Laptop",
            "Business laptop",
            CreateValidMoney());
    }

    private static Money CreateValidMoney()
    {
        return Money.Create(
            1000m,
            "USD");
    }

    private static Money CreateAnotherMoney()
    {
        return Money.Create(
            1500m,
            "USD");
    }

    #endregion
}
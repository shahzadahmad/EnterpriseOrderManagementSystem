using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Application.Common.Models;
using EnterpriseOrderManagementSystem.Application.Features.Products.Commands;
using EnterpriseOrderManagementSystem.Application.Features.Products.Queries;
using EnterpriseOrderManagementSystem.Application.Abstractions.Persistence;
using EnterpriseOrderManagementSystem.Application.Tests.Common;
using EnterpriseOrderManagementSystem.Domain.Aggregates.ProductAggregate;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using Moq;

namespace EnterpriseOrderManagementSystem.Application.Tests.Features.Products;

public sealed class ProductHandlerTests
{
    [Fact]
    public async Task CreateProduct_ShouldCreateAndPersistProduct()
    {
        var repo = new Mock<IProductRepository>();
        repo.Setup(x => x.ExistsBySkuAsync("SKU-100", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        Product? added = null;
        repo.Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((p, _) => added = p).Returns(Task.CompletedTask);

        var result = await new CreateProductCommandHandler(repo.Object).Handle(
            new CreateProductCommand("SKU-100", "Product", "Description", 20m, "USD"),
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result);
        Assert.Equal(result, added!.Id);
        Assert.Equal("SKU-100", added.SKU);
        Assert.Equal(20m, added.Price.Amount);
    }

    [Fact]
    public async Task CreateProduct_ShouldThrowConflictWhenSkuExists()
    {
        var repo = new Mock<IProductRepository>();
        repo.Setup(x => x.ExistsBySkuAsync("SKU-100", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() =>
            new CreateProductCommandHandler(repo.Object).Handle(
                new CreateProductCommand("SKU-100", "Product", "Description", 20m, "USD"),
                CancellationToken.None));
        repo.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProduct_ShouldUpdateDetails()
    {
        var product = TestData.Product();
        var repo = Repo(product);
        var result = await new UpdateProductCommandHandler(repo.Object).Handle(
            new UpdateProductCommand(product.Id, "Updated", "Updated description"), CancellationToken.None);
        Assert.Equal(product.Id, result);
        Assert.Equal("Updated", product.Name);
        Assert.Equal("Updated description", product.Description);
    }

    [Fact]
    public async Task ChangeProductPrice_ShouldChangePrice()
    {
        var product = TestData.Product(10m);
        var repo = Repo(product);
        var result = await new ChangeProductPriceCommandHandler(repo.Object).Handle(
            new ChangeProductPriceCommand(product.Id, 25m, "USD"), CancellationToken.None);
        Assert.Equal(product.Id, result);
        Assert.Equal(25m, product.Price.Amount);
    }

    [Fact]
    public async Task ActivateProduct_ShouldActivateProduct()
    {
        var product = TestData.Product();
        product.Deactivate();
        var repo = Repo(product);
        var result = await new ActivateProductCommandHandler(repo.Object).Handle(
            new ActivateProductCommand(product.Id), CancellationToken.None);
        Assert.Equal(product.Id, result);
        Assert.Equal(Domain.Enums.ProductStatus.Active, product.Status);
    }

    [Fact]
    public async Task DeactivateProduct_ShouldDeactivateProduct()
    {
        var product = TestData.Product();
        var repo = Repo(product);
        var result = await new DeactivateProductCommandHandler(repo.Object).Handle(
            new DeactivateProductCommand(product.Id), CancellationToken.None);
        Assert.Equal(product.Id, result);
        Assert.Equal(Domain.Enums.ProductStatus.Inactive, product.Status);
    }

    [Fact]
    public async Task DiscontinueProduct_ShouldDiscontinueProduct()
    {
        var product = TestData.Product();
        var repo = Repo(product);
        var result = await new DiscontinueProductCommandHandler(repo.Object).Handle(
            new DiscontinueProductCommand(product.Id), CancellationToken.None);
        Assert.Equal(product.Id, result);
        Assert.Equal(Domain.Enums.ProductStatus.Discontinued, product.Status);
    }

    [Fact]
    public async Task GetProductById_ShouldReturnMappedDto()
    {
        var product = TestData.Product(35m);
        var repo = Repo(product);
        var result = await new GetProductByIdQueryHandler(repo.Object).Handle(
            new GetProductByIdQuery(product.Id), CancellationToken.None);
        Assert.NotNull(result);
        Assert.Equal(product.Id, result!.Id);
        Assert.Equal(35m, result.Price);
        Assert.Equal("USD", result.Currency);
    }

    [Fact]
    public async Task GetProductById_ShouldReturnNullWhenMissing()
    {
        var id = Guid.NewGuid();
        var repo = Repo(null);
        Assert.Null(await new GetProductByIdQueryHandler(repo.Object).Handle(
            new GetProductByIdQuery(id), CancellationToken.None));
    }

    [Fact]
    public async Task GetProducts_ShouldReturnPagedMappedResults()
    {
        var products = new[] { TestData.Product(), TestData.Product(50m) };
        var repo = new Mock<IReadRepository<Product>>();
        repo.Setup(x => x.CountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(8);
        repo.Setup(x => x.ListAsync(5, 5, It.IsAny<CancellationToken>())).ReturnsAsync(products);

        var result = await new GetProductsQueryHandler(repo.Object).Handle(
            new GetProductsQuery(new PaginationRequest(2, 5)), CancellationToken.None);

        Assert.Equal(8, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.PageNumber);
    }

    [Fact]
    public async Task ProductCommands_ShouldThrowNotFoundWhenProductDoesNotExist()
    {
        var id = Guid.NewGuid();
        var repo = Repo(null);
        await Assert.ThrowsAsync<NotFoundException>(() => new ActivateProductCommandHandler(repo.Object)
            .Handle(new ActivateProductCommand(id), CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(() => new DeactivateProductCommandHandler(repo.Object)
            .Handle(new DeactivateProductCommand(id), CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(() => new DiscontinueProductCommandHandler(repo.Object)
            .Handle(new DiscontinueProductCommand(id), CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(() => new UpdateProductCommandHandler(repo.Object)
            .Handle(new UpdateProductCommand(id, "Name", "Description"), CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(() => new ChangeProductPriceCommandHandler(repo.Object)
            .Handle(new ChangeProductPriceCommand(id, 10m, "USD"), CancellationToken.None));
    }

    private static Mock<IProductRepository> Repo(Product? product)
    {
        var repo = new Mock<IProductRepository>();
        if (product is null)
            repo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);
        else
            repo.Setup(x => x.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        return repo;
    }
}

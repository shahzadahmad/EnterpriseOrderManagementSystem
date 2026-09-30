using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Aggregates.ProductAggregate;
using EnterpriseOrderManagementSystem.Domain.Common.ValueObjects;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Products.Commands;

public sealed class CreateProductCommandHandler
    : IRequestHandler<CreateProductCommand, Guid>
{
    #region Fields

    private readonly IProductRepository _productRepository;

    #endregion

    #region Constructor

    public CreateProductCommandHandler(
        IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    #endregion

    #region Handle

    public async Task<Guid> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var skuExists = await _productRepository.ExistsBySkuAsync(
            request.SKU,
            cancellationToken);

        if (skuExists)
        {
            throw new ConflictException(
                $"Product with SKU '{request.SKU}' already exists.");
        }

        var price = Money.Create(
            request.Price,
            request.Currency);

        var product = Product.Create(
            request.SKU,
            request.Name,
            request.Description,
            price);

        await _productRepository.AddAsync(
            product,
            cancellationToken);

        return product.Id;
    }

    #endregion
}
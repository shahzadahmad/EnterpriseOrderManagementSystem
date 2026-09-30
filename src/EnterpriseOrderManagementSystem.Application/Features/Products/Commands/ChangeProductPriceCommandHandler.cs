using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Common.ValueObjects;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Products.Commands;

public sealed class ChangeProductPriceCommandHandler
    : IRequestHandler<ChangeProductPriceCommand, Guid>
{
    #region Fields

    private readonly IProductRepository _productRepository;

    #endregion

    #region Constructor

    public ChangeProductPriceCommandHandler(
        IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    #endregion

    #region Handle

    public async Task<Guid> Handle(
        ChangeProductPriceCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(
            request.ProductId,
            cancellationToken);

        if (product is null)
        {
            throw new NotFoundException(
                "Product",
                request.ProductId);
        }

        var newPrice = Money.Create(
            request.Price,
            request.Currency);

        product.ChangePrice(newPrice);

        return product.Id;
    }

    #endregion
}
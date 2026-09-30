using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Products.Commands;

public sealed class UpdateProductCommandHandler
    : IRequestHandler<UpdateProductCommand, Guid>
{
    #region Fields

    private readonly IProductRepository _productRepository;

    #endregion

    #region Constructor

    public UpdateProductCommandHandler(
        IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    #endregion

    #region Handle

    public async Task<Guid> Handle(
        UpdateProductCommand request,
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

        product.UpdateDetails(
            request.Name,
            request.Description);

        return product.Id;
    }

    #endregion
}
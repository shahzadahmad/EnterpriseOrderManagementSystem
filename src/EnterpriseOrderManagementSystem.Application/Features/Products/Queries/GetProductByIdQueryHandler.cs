using EnterpriseOrderManagementSystem.Application.DTOs.Products;
using EnterpriseOrderManagementSystem.Application.Mapping;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Products.Queries;

public sealed class GetProductByIdQueryHandler
    : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    #region Fields

    private readonly IProductRepository _productRepository;

    #endregion

    #region Constructor

    public GetProductByIdQueryHandler(
        IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    #endregion

    #region Handle

    public async Task<ProductDto?> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(
            request.ProductId,
            cancellationToken);

        return product?.ToDto();
    }

    #endregion
}
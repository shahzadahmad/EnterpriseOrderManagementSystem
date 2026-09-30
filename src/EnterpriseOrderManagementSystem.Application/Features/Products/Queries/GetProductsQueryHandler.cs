using EnterpriseOrderManagementSystem.Application.Abstractions.Persistence;
using EnterpriseOrderManagementSystem.Application.Common.Models;
using EnterpriseOrderManagementSystem.Application.DTOs.Products;
using EnterpriseOrderManagementSystem.Application.Mapping;
using EnterpriseOrderManagementSystem.Domain.Aggregates.ProductAggregate;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Products.Queries;

public sealed class GetProductsQueryHandler
    : IRequestHandler<
        GetProductsQuery,
        PaginatedResult<ProductDto>>
{
    #region Fields

    private readonly IReadRepository<Product> _readRepository;

    #endregion

    #region Constructor

    public GetProductsQueryHandler(
        IReadRepository<Product> readRepository)
    {
        _readRepository = readRepository;
    }

    #endregion

    #region Handle

    public async Task<PaginatedResult<ProductDto>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        var pagination = request.Pagination;

        var totalCount = await _readRepository.CountAsync(
            cancellationToken);

        var products = await _readRepository.ListAsync(
            pagination.Skip,
            pagination.PageSize,
            cancellationToken);

        var items = products
            .Select(product => product.ToDto())
            .ToArray();

        return new PaginatedResult<ProductDto>(
            items,
            pagination.PageNumber,
            pagination.PageSize,
            totalCount);
    }

    #endregion
}
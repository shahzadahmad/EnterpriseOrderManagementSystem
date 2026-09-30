using EnterpriseOrderManagementSystem.Application.Abstractions.Persistence;
using EnterpriseOrderManagementSystem.Application.Common.Models;
using EnterpriseOrderManagementSystem.Application.DTOs.Inventory;
using EnterpriseOrderManagementSystem.Application.Mapping;
using InventoryEntity = EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Inventory.Queries;

public sealed class GetInventoriesQueryHandler
    : IRequestHandler<
        GetInventoriesQuery,
        PaginatedResult<InventoryDto>>
{
    #region Fields

    private readonly IReadRepository<InventoryEntity.Inventory> _readRepository;

    #endregion

    #region Constructor

    public GetInventoriesQueryHandler(
        IReadRepository<InventoryEntity.Inventory> readRepository)
    {
        _readRepository = readRepository;
    }

    #endregion

    #region Handle

    public async Task<PaginatedResult<InventoryDto>> Handle(
        GetInventoriesQuery request,
        CancellationToken cancellationToken)
    {
        var pagination = request.Pagination;

        var totalCount = await _readRepository.CountAsync(
            cancellationToken);

        var inventories = await _readRepository.ListAsync(
            pagination.Skip,
            pagination.PageSize,
            cancellationToken);

        var items = inventories
            .Select(inventory => inventory.ToDto())
            .ToArray();

        return new PaginatedResult<InventoryDto>(
            items,
            pagination.PageNumber,
            pagination.PageSize,
            totalCount);
    }

    #endregion
}
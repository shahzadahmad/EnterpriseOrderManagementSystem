using EnterpriseOrderManagementSystem.Application.DTOs.Inventory;
using EnterpriseOrderManagementSystem.Application.Mapping;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Inventory.Queries;

public sealed class GetInventoryByIdQueryHandler
    : IRequestHandler<GetInventoryByIdQuery, InventoryDto?>
{
    #region Fields

    private readonly IInventoryRepository _inventoryRepository;

    #endregion

    #region Constructor

    public GetInventoryByIdQueryHandler(
        IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    #endregion

    #region Handle

    public async Task<InventoryDto?> Handle(
        GetInventoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        var inventory = await _inventoryRepository.GetByIdAsync(
            request.InventoryId,
            cancellationToken);

        return inventory?.ToDto();
    }

    #endregion

}
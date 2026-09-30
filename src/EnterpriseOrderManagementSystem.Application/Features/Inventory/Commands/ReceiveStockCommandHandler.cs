using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Inventory.Commands;

public sealed class ReceiveStockCommandHandler
    : IRequestHandler<ReceiveStockCommand, Guid>
{
    #region Fields

    private readonly IInventoryRepository _inventoryRepository;

    #endregion

    #region Constructor

    public ReceiveStockCommandHandler(
        IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    #endregion

    #region Handle

    public async Task<Guid> Handle(
        ReceiveStockCommand request,
        CancellationToken cancellationToken)
    {
        var inventory = await _inventoryRepository.GetByIdAsync(
            request.InventoryId,
            cancellationToken);

        if (inventory is null)
        {
            throw new NotFoundException(
                "Inventory",
                request.InventoryId);
        }

        inventory.ReceiveStock(
            request.Quantity,
            request.Reference,
            request.Reason);

        return inventory.Id;
    }

    #endregion
}
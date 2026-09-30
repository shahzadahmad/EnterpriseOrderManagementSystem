using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Inventory.Commands;

public sealed class DiscontinueInventoryCommandHandler
    : IRequestHandler<DiscontinueInventoryCommand, Guid>
{
    #region Fields

    private readonly IInventoryRepository _inventoryRepository;

    #endregion

    #region Constructor

    public DiscontinueInventoryCommandHandler(
        IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    #endregion

    #region Handle

    public async Task<Guid> Handle(
        DiscontinueInventoryCommand request,
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

        inventory.Discontinue(request.Reason);

        return inventory.Id;
    }

    #endregion
}

using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using MediatR;
using System.Diagnostics;
using InventoryAggregate =
    EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate.Inventory;

namespace EnterpriseOrderManagementSystem.Application.Features.Inventory.Commands;

/// <summary>
/// Handles inventory creation while enforcing product/warehouse uniqueness.
/// </summary>
public sealed class CreateInventoryCommandHandler
    : IRequestHandler<CreateInventoryCommand, Guid>
{
    private readonly IInventoryRepository _inventoryRepository;

    public CreateInventoryCommandHandler(
        IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public async Task<Guid> Handle(
     CreateInventoryCommand request,
     CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        Console.WriteLine("Inventory creation started.");

        var alreadyExists =
            await _inventoryRepository.ExistsByProductAndWarehouseAsync(
                request.ProductId,
                request.WarehouseId,
                cancellationToken);

        Console.WriteLine(
            $"Exists check completed: {stopwatch.ElapsedMilliseconds} ms");

        if (alreadyExists)
        {
            throw new ConflictException(
                "Inventory already exists for this product and warehouse.");
        }

        var inventory = InventoryAggregate.Create(
            Guid.NewGuid(),
            request.ProductId,
            request.WarehouseId,
            request.ReorderLevel,
            request.MaximumQuantity);

        await _inventoryRepository.AddAsync(
            inventory,
            cancellationToken);

        Console.WriteLine(
            $"Inventory added: {stopwatch.ElapsedMilliseconds} ms");

        return inventory.Id;
    }
}

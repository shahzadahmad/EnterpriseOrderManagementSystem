using EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using EnterpriseOrderManagementSystem.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseOrderManagementSystem.Infrastructure.Persistence.Repositories;

public sealed class InventoryRepository
    : IInventoryRepository
{
    private readonly AppDbContext _context;

    public InventoryRepository(
        AppDbContext context)
    {
        _context = context;
    }

    #region Get By Id

    public async Task<Inventory?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Inventories
            .Include(x => x.Transactions)
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    #endregion

    #region Exists

    public async Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Inventories
            .AnyAsync(
                x => x.Id == id,
                cancellationToken);
    }

    #endregion

    #region Get By Product And Warehouse

    public async Task<Inventory?> GetByProductAndWarehouseAsync(
        Guid productId,
        Guid warehouseId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Inventories
            .Include(x => x.Transactions)
            .FirstOrDefaultAsync(
                x =>
                    x.ProductId == productId &&
                    x.WarehouseId == warehouseId,
                cancellationToken);
    }

    #endregion

    #region Exists By Product And Warehouse

    public async Task<bool> ExistsByProductAndWarehouseAsync(
        Guid productId,
        Guid warehouseId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Inventories
            .AnyAsync(
                x =>
                    x.ProductId == productId &&
                    x.WarehouseId == warehouseId,
                cancellationToken);
    }

    #endregion

    #region Add

    public async Task AddAsync(
        Inventory aggregate,
        CancellationToken cancellationToken = default)
    {
        await _context.Inventories.AddAsync(
            aggregate,
            cancellationToken);
    }

    #endregion
}
using EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using EnterpriseOrderManagementSystem.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseOrderManagementSystem.Infrastructure.Persistence.Repositories;

public sealed class OrderRepository
    : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(
        AppDbContext context)
    {
        _context = context;
    }

    #region Get By Id

    public async Task<Order?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(x => x.Items)
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
        return await _context.Orders
            .AnyAsync(
                x => x.Id == id,
                cancellationToken);
    }

    #endregion

    #region Get By Order Number

    public async Task<Order?> GetByOrderNumberAsync(
        string orderNumber,
        CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(x => x.Items)
            .FirstOrDefaultAsync(
                x => x.OrderNumber == orderNumber,
                cancellationToken);
    }

    #endregion

    #region Exists By Order Number

    public async Task<bool> ExistsByOrderNumberAsync(
        string orderNumber,
        CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .AnyAsync(
                x => x.OrderNumber == orderNumber,
                cancellationToken);
    }

    #endregion

    #region Add

    public async Task AddAsync(
        Order aggregate,
        CancellationToken cancellationToken = default)
    {
        await _context.Orders.AddAsync(
            aggregate,
            cancellationToken);
    }

    #endregion
}
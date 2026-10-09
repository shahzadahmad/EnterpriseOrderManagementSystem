using EnterpriseOrderManagementSystem.Domain.Aggregates.PaymentAggregate;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using EnterpriseOrderManagementSystem.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseOrderManagementSystem.Infrastructure.Persistence.Repositories;

public sealed class PaymentRepository
    : IPaymentRepository
{
    private readonly AppDbContext _context;

    public PaymentRepository(
        AppDbContext context)
    {
        _context = context;
    }

    #region Get By Id

    public async Task<Payment?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Payments
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
        return await _context.Payments
            .AnyAsync(
                x => x.Id == id,
                cancellationToken);
    }

    #endregion

    #region Get By Order Id

    public async Task<Payment?> GetByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(x => x.Transactions)
            .FirstOrDefaultAsync(
                x => x.OrderId == orderId,
                cancellationToken);
    }

    #endregion

    #region Exists By Order Id

    public async Task<bool> ExistsByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .AnyAsync(
                x => x.OrderId == orderId,
                cancellationToken);
    }

    #endregion

    #region Add

    public async Task AddAsync(
        Payment aggregate,
        CancellationToken cancellationToken = default)
    {
        await _context.Payments.AddAsync(
            aggregate,
            cancellationToken);
    }

    #endregion
}
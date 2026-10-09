using EnterpriseOrderManagementSystem.Domain.Repositories;
using EnterpriseOrderManagementSystem.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore.Storage;

namespace EnterpriseOrderManagementSystem.Infrastructure.Persistence.Transactions;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    private IDbContextTransaction? _currentTransaction;

    public UnitOfWork(
        AppDbContext context)
    {
        _context = context;
    }

    #region Begin Transaction

    public async Task BeginTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
        {
            throw new InvalidOperationException(
                "A database transaction is already active.");
        }

        _currentTransaction =
            await _context.Database.BeginTransactionAsync(
                cancellationToken);
    }

    #endregion

    #region Save Changes

    public async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(
            cancellationToken);
    }

    #endregion

    #region Commit Transaction

    public async Task CommitTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
        {
            throw new InvalidOperationException(
                "No active database transaction exists.");
        }

        try
        {
            await _currentTransaction.CommitAsync(
                cancellationToken);
        }
        finally
        {
            await _currentTransaction.DisposeAsync();

            _currentTransaction = null;
        }
    }

    #endregion

    #region Rollback Transaction

    public async Task RollbackTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
        {
            return;
        }

        try
        {
            await _currentTransaction.RollbackAsync(
                cancellationToken);
        }
        finally
        {
            await _currentTransaction.DisposeAsync();

            _currentTransaction = null;
        }
    }

    #endregion
}
using EnterpriseOrderManagementSystem.Application.Abstractions.Persistence;
using EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate;
using EnterpriseOrderManagementSystem.Domain.Common.Base;
using EnterpriseOrderManagementSystem.Domain.Common.Events;
using EnterpriseOrderManagementSystem.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace EnterpriseOrderManagementSystem.Infrastructure.Persistence.Transactions;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _currentTransaction;
    private readonly ILogger<UnitOfWork> _logger;

    public UnitOfWork(
        AppDbContext context,
        ILogger<UnitOfWork> logger)
    {
        _context = context;
        _logger = logger;
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
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            foreach (var entry in ex.Entries)
            {
                _logger.LogError(
                    "Concurrency exception. Entity: {Entity}, State: {State}",
                    entry.Metadata.ClrType.Name,
                    entry.State);

                foreach (var property in entry.Properties)
                {
                    _logger.LogError(
                        "Property: {Property}, Original: {Original}, Current: {Current}",
                        property.Metadata.Name,
                        property.OriginalValue,
                        property.CurrentValue);
                }

                var databaseValues =
                    await entry.GetDatabaseValuesAsync(cancellationToken);

                if (databaseValues == null)
                {
                    _logger.LogError(
                        "Entity {Entity} no longer exists in the database.",
                        entry.Metadata.ClrType.Name);
                }
                else
                {
                    _logger.LogError(
                        "Entity {Entity} still exists in the database.",
                        entry.Metadata.ClrType.Name);
                }
            }

            throw;
        }
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

    #region Domain Events

    public IReadOnlyCollection<IDomainEvent> GetDomainEvents()
    {
        return _context.ChangeTracker
            .Entries()
            .Select(entry => entry.Entity)
            .OfType<IHasDomainEvents>()
            .SelectMany(aggregate => aggregate.DomainEvents)
            .ToArray();
    }

    public void ClearDomainEvents()
    {
        foreach (var aggregate in _context.ChangeTracker
            .Entries()
            .Select(entry => entry.Entity)
            .OfType<IHasDomainEvents>())
        {
            aggregate.ClearDomainEvents();
        }
    }

    #endregion
}
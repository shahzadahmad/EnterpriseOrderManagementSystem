using EnterpriseOrderManagementSystem.Application.Abstractions.Persistence;
using EnterpriseOrderManagementSystem.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseOrderManagementSystem.Infrastructure.Persistence.Repositories;

public sealed class EfReadRepository<T>
    : IReadRepository<T>
    where T : class
{
    private readonly AppDbContext _context;

    public EfReadRepository(
        AppDbContext context)
    {
        _context = context;
    }

    #region Get By Id

    public async Task<T?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context
            .Set<T>()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                entity => EF.Property<Guid>(entity, "Id") == id,
                cancellationToken);
    }

    #endregion

    #region List

    public async Task<IReadOnlyCollection<T>> ListAsync(
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        return await _context
            .Set<T>()
            .AsNoTracking()
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    #endregion

    #region Count

    public async Task<int> CountAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context
            .Set<T>()
            .AsNoTracking()
            .CountAsync(cancellationToken);
    }

    #endregion
}
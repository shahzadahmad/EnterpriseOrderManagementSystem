using EnterpriseOrderManagementSystem.Domain.Aggregates.CustomerAggregate;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using EnterpriseOrderManagementSystem.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseOrderManagementSystem.Infrastructure.Persistence.Repositories;

public sealed class CustomerRepository
    : ICustomerRepository
{
    private readonly AppDbContext _context;

    public CustomerRepository(
        AppDbContext context)
    {
        _context = context;
    }

    #region Get By Id

    public async Task<Customer?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Customers
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
        return await _context.Customers
            .AnyAsync(
                x => x.Id == id,
                cancellationToken);
    }

    #endregion

    #region Get By Email

    public async Task<Customer?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(
                x => x.Email.Value == email,
                cancellationToken);
    }

    #endregion

    #region Exists By Email

    public async Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .AnyAsync(
                x => x.Email.Value == email,
                cancellationToken);
    }

    #endregion

    #region Add

    public async Task AddAsync(
        Customer aggregate,
        CancellationToken cancellationToken = default)
    {
        await _context.Customers.AddAsync(
            aggregate,
            cancellationToken);
    }

    #endregion
}
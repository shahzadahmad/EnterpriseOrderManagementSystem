using EnterpriseOrderManagementSystem.Domain.Aggregates.ProductAggregate;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using EnterpriseOrderManagementSystem.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseOrderManagementSystem.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository
    : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(
        AppDbContext context)
    {
        _context = context;
    }

    #region Get By Id

    public async Task<Product?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Products
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
        return await _context.Products
            .AnyAsync(
                x => x.Id == id,
                cancellationToken);
    }

    #endregion

    #region Get By SKU

    public async Task<Product?> GetBySkuAsync(
        string sku,
        CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .FirstOrDefaultAsync(
                x => x.SKU == sku,
                cancellationToken);
    }

    #endregion

    #region Exists By SKU

    public async Task<bool> ExistsBySkuAsync(
        string sku,
        CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AnyAsync(
                x => x.SKU == sku,
                cancellationToken);
    }

    #endregion

    #region Add

    public async Task AddAsync(
        Product aggregate,
        CancellationToken cancellationToken = default)
    {
        await _context.Products.AddAsync(
            aggregate,
            cancellationToken);
    }

    #endregion
}
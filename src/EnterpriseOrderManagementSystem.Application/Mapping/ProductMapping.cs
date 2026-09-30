using EnterpriseOrderManagementSystem.Application.DTOs.Products;
using EnterpriseOrderManagementSystem.Domain.Aggregates.ProductAggregate;

namespace EnterpriseOrderManagementSystem.Application.Mapping;

public static class ProductMapping
{
    public static ProductDto ToDto(this Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        return new ProductDto(
            product.Id,
            product.SKU,
            product.Name,
            product.Description,
            product.Price.Amount,
            product.Price.Currency,
            product.Status,
            product.CreatedOnUtc);
    }
}
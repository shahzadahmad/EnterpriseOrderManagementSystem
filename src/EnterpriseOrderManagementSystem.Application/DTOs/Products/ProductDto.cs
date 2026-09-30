using EnterpriseOrderManagementSystem.Domain.Enums;

namespace EnterpriseOrderManagementSystem.Application.DTOs.Products;

public sealed record ProductDto(
    Guid Id,
    string SKU,
    string Name,
    string Description,
    decimal Price,
    string Currency,
    ProductStatus Status,
    DateTime CreatedOnUtc);

using System.ComponentModel.DataAnnotations;

namespace EnterpriseOrderManagementSystem.API.Contracts.Products;

public sealed record CreateProductRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string SKU { get; init; } = string.Empty;

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [StringLength(2000, MinimumLength = 1)]
    public string Description { get; init; } = string.Empty;

    [Range(typeof(decimal), "0.01", "999999999999")]
    public decimal Price { get; init; }

    [Required]
    [StringLength(10, MinimumLength = 1)]
    public string Currency { get; init; } = string.Empty;
}

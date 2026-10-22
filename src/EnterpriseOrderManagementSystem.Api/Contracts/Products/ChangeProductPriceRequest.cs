
using System.ComponentModel.DataAnnotations;

namespace EnterpriseOrderManagementSystem.API.Contracts.Products;

public sealed record ChangeProductPriceRequest
{
    [Range(typeof(decimal), "0.01", "999999999999")]
    public decimal Price { get; init; }

    [Required]
    [StringLength(10, MinimumLength = 1)]
    public string Currency { get; init; } = string.Empty;
}

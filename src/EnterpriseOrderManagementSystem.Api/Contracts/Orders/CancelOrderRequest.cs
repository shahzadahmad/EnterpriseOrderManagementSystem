
using System.ComponentModel.DataAnnotations;

namespace EnterpriseOrderManagementSystem.API.Contracts.Orders;

public sealed record CancelOrderRequest
{
    [Required]
    [StringLength(500, MinimumLength = 1)]
    public string Reason { get; init; } = string.Empty;
}

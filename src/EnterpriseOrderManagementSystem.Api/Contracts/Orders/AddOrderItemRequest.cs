
using System.ComponentModel.DataAnnotations;

namespace EnterpriseOrderManagementSystem.API.Contracts.Orders;

public sealed record AddOrderItemRequest
{
    [Required]
    public Guid ProductId { get; init; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; init; }
}

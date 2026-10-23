
using System.ComponentModel.DataAnnotations;

namespace EnterpriseOrderManagementSystem.API.Contracts.Orders;

public sealed record CreateOrderRequest
{
    [Required]
    public Guid CustomerId { get; init; }
}

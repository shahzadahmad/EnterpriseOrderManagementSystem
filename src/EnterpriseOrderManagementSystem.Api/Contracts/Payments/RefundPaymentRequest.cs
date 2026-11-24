using System.ComponentModel.DataAnnotations;

namespace EnterpriseOrderManagementSystem.API.Contracts.Payments;

public sealed record RefundPaymentRequest
{
    [Range(typeof(decimal), "0.01", "999999999")]
    public decimal RefundAmount { get; init; }

    [Required]
    [StringLength(500, MinimumLength = 1)]
    public string Reason { get; init; } = string.Empty;
}
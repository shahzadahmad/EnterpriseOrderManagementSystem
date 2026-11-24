using System.ComponentModel.DataAnnotations;
using EnterpriseOrderManagementSystem.Domain.Enums;

namespace EnterpriseOrderManagementSystem.API.Contracts.Payments;

public sealed record CreatePaymentRequest
{
    [Required]
    public Guid OrderId { get; init; }

    [Range(typeof(decimal), "0.01", "999999999")]
    public decimal Amount { get; init; }

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string Currency { get; init; } = "USD";

    [EnumDataType(typeof(PaymentMethod))]
    public PaymentMethod PaymentMethod { get; init; }

    [EnumDataType(typeof(PaymentProvider))]
    public PaymentProvider PaymentProvider { get; init; }
}
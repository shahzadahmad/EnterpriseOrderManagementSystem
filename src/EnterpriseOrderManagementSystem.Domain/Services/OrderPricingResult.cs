namespace EnterpriseOrderManagementSystem.Domain.Services;

/// <summary>
/// Represents the result of calculating pricing
/// for an order.
///
/// This object contains the calculated monetary values
/// but does not modify the Order aggregate.
/// </summary>
public sealed record OrderPricingResult(
    decimal SubTotal,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal TotalAmount);
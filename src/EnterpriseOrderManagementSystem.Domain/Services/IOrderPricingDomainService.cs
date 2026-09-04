using EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.ProductAggregate;

namespace EnterpriseOrderManagementSystem.Domain.Services;

/// <summary>
/// Defines domain-level pricing calculations for orders.
///
/// The service is used when pricing requires information
/// from outside the Order aggregate.
/// </summary>
public interface IOrderPricingDomainService
{
    /// <summary>
    /// Calculates the pricing information for an order.
    /// </summary>
    OrderPricingResult Calculate(
        Order order,
        IReadOnlyCollection<Product> products);
}
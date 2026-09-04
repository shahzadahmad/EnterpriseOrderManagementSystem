using EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.ProductAggregate;
using EnterpriseOrderManagementSystem.Domain.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Common.ValueObjects;

namespace EnterpriseOrderManagementSystem.Domain.Services;

/// <summary>
/// Calculates order pricing using information from
/// the Order and Product aggregates.
///
/// This service does not modify the aggregates.
/// </summary>
public sealed class OrderPricingDomainService
    : IOrderPricingDomainService
{
    #region Public Methods

    /// <summary>
    /// Calculates the subtotal, discount, tax and final
    /// total for an order.
    /// </summary>
    public OrderPricingResult Calculate(
        Order order,
        IReadOnlyCollection<Product> products)
    {
        ArgumentNullException.ThrowIfNull(order);

        ArgumentNullException.ThrowIfNull(products);

        #region Validate Order

        if (order.Items.Count == 0)
        {
            throw new BusinessRuleViolationException(
                "An order must contain at least one item before pricing can be calculated.");
        }

        #endregion

        #region Calculate Subtotal

        Decimal subtotal = 0m;

        foreach (var orderItem in order.Items)
        {
            var product = products.FirstOrDefault(x => x.Id == orderItem.ProductId);

            if (product is null)
            {
                throw new BusinessRuleViolationException(
                    $"Product '{orderItem.ProductId}' was not found.");
            }

            /*
             * The OrderItem remains responsible for its own
             * line-level calculation.
             *
             * We therefore use the item's existing pricing
             * information rather than duplicating that
             * calculation here.
             */
            subtotal += Convert.ToDecimal(orderItem.LineTotal.Amount);
        }

        #endregion

        #region Calculate Discount

        var discountAmount =
            CalculateDiscount(
                order,
                subtotal);

        #endregion

        #region Calculate Tax

        var taxableAmount =
            subtotal - discountAmount;

        var taxAmount =
            CalculateTax(
                taxableAmount);

        #endregion

        #region Calculate Total

        var totalAmount =
            taxableAmount + taxAmount;

        #endregion

        return new OrderPricingResult(
            SubTotal: subtotal,
            DiscountAmount: discountAmount,
            TaxAmount: taxAmount,
            TotalAmount: totalAmount);
    }

    #endregion

    #region Pricing Rules

    /// <summary>
    /// Calculates the applicable discount.
    ///
    /// The current implementation contains no discount
    /// policy and therefore returns zero.
    ///
    /// This method gives us a dedicated location for
    /// future domain discount rules.
    /// </summary>
    private static decimal CalculateDiscount(
        Order order,
        decimal subtotal)
    {
        _ = order;

        if (subtotal <= 0m)
        {
            return 0m;
        }

        return 0m;
    }

    /// <summary>
    /// Calculates applicable tax.
    ///
    /// The current Domain model does not yet define a
    /// tax policy, so no tax is applied here.
    ///
    /// A real tax policy can be introduced later without
    /// moving persistence or infrastructure concerns into
    /// the Domain layer.
    /// </summary>
    private static decimal CalculateTax(
        decimal taxableAmount)
    {
        if (taxableAmount <= 0m)
        {
            return 0m;
        }

        return 0m;
    }

    #endregion
}
using EnterpriseOrderManagementSystem.Domain.Common.Base;
using EnterpriseOrderManagementSystem.Domain.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Common.ValueObjects;

namespace EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate;

/// <summary>
/// Represents a single product line within an Order.
///
/// This is a child Entity of the Order Aggregate.
/// It cannot exist independently and should only be
/// created or modified through the Order Aggregate Root.
/// </summary>
public sealed class OrderItem : Entity<Guid>
{
    #region Constructors

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    private OrderItem()
    {
    }

    private OrderItem(
        Guid orderItemId,
        Guid productId,
        string productName,
        Money unitPrice,
        int quantity)
    {
        Id = orderItemId;

        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;

        SetQuantity(quantity);
    }

    #endregion

    #region Properties

    /// <summary>
    /// Product identifier.
    /// </summary>
    public Guid ProductId { get; private set; }

    /// <summary>
    /// Product name at the time the order was placed.
    /// Stored for historical accuracy.
    /// </summary>
    public string ProductName { get; private set; } = string.Empty;

    /// <summary>
    /// Price of one unit when the order was placed.
    /// </summary>
    public Money UnitPrice { get; private set; } = default!;

    /// <summary>
    /// Ordered quantity.
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// Total amount for this order line.
    /// (Quantity × UnitPrice)
    /// </summary>
    public Money LineTotal { get; private set; } = default!;

    #endregion

    #region Factory

    /// <summary>
    /// Creates a new OrderItem.
    /// Only the Order Aggregate should call this method.
    /// </summary>
    internal static OrderItem Create(
        Guid orderItemId,
        Guid productId,
        string productName,
        Money unitPrice,
        int quantity)
    {
        if (orderItemId == Guid.Empty)
            throw new BusinessRuleViolationException(
                "OrderItem Id cannot be empty.");

        if (productId == Guid.Empty)
            throw new BusinessRuleViolationException(
                "Product Id cannot be empty.");

        ArgumentException.ThrowIfNullOrWhiteSpace(productName);

        if (unitPrice is null)
            throw new BusinessRuleViolationException(
                "Unit price cannot be null.");

        if (unitPrice.Amount < 0)
            throw new BusinessRuleViolationException(
                "Unit price cannot be negative.");

        return new OrderItem(
            orderItemId,
            productId,
            productName.Trim(),
            unitPrice,
            quantity);
    }

    #endregion

    #region Quantity Management

    /// <summary>
    /// Increases the quantity.
    /// Only the Aggregate Root should call this.
    /// </summary>
    internal void IncreaseQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new BusinessRuleViolationException(
                "Quantity must be greater than zero.");
        }

        Quantity += quantity;

        RecalculateLineTotal();
    }

    /// <summary>
    /// Decreases the quantity.
    /// </summary>
    internal void DecreaseQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new BusinessRuleViolationException(
                "Quantity must be greater than zero.");
        }

        if (Quantity - quantity <= 0)
        {
            throw new BusinessRuleViolationException(
                "Quantity cannot become zero or negative.");
        }

        Quantity -= quantity;

        RecalculateLineTotal();
    }

    /// <summary>
    /// Sets a new quantity.
    /// Used by the Aggregate Root.
    /// </summary>
    internal void SetQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new BusinessRuleViolationException(
                "Quantity must be greater than zero.");
        }

        Quantity = quantity;

        RecalculateLineTotal();
    }

    #endregion

    #region Price Management

    /// <summary>
    /// Updates the unit price.
    /// Normally used during business operations such as
    /// recalculation before order confirmation.
    /// </summary>
    internal void ChangeUnitPrice(Money newPrice)
    {
        UnitPrice = newPrice;

        RecalculateLineTotal();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Recalculates the total value of this order line.
    /// </summary>
    private void RecalculateLineTotal()
    {
        LineTotal = UnitPrice * Quantity;
    }

    #endregion
}
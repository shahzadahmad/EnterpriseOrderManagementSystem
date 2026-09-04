using EnterpriseOrderManagementSystem.Domain.Common.Base;
using EnterpriseOrderManagementSystem.Domain.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Common.ValueObjects;
using EnterpriseOrderManagementSystem.Domain.Enums;
using EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate.Events;

namespace EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate;

/// <summary>
/// Represents a customer's order.
///
/// This is the Aggregate Root for the Order domain.
/// All modifications to OrderItems must go through this class.
///
/// Responsibilities:
/// - Create orders
/// - Add/Remove items
/// - Calculate totals
/// - Manage order state
/// - Raise domain events
/// - Protect business invariants
/// </summary>
public sealed class Order : AggregateRoot<Guid>
{
    /// <summary>
    /// Backing collection for order items.
    /// External code cannot modify this collection directly.
    /// </summary>
    private readonly List<OrderItem> _items = [];

    #region Constructors

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    private Order()
    {
    }

    private Order(Guid customerId)
    {
        Id = Guid.NewGuid();

        CustomerId = customerId;

        Status = OrderStatus.Pending;

        TotalAmount = Money.Zero("USD");
    }

    #endregion

    #region Properties

    /// <summary>
    /// Customer that owns this order.
    /// </summary>
    public Guid CustomerId { get; private set; }

    /// <summary>
    /// Current order status.
    /// </summary>
    public OrderStatus Status { get; private set; }

    /// <summary>
    /// Total amount of the order.
    /// </summary>
    public Money TotalAmount { get; private set; }

    /// <summary>
    /// Read-only collection of order items.
    /// </summary>
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    #endregion

    #region Factory

    /// <summary>
    /// Creates a new order.
    /// </summary>
    public static Order Create(Guid customerId)
    {
        if (customerId == Guid.Empty)
        {
            throw new BusinessRuleViolationException(
                "CustomerId cannot be empty.");
        }

        var order = new Order(customerId);

        order.AddDomainEvent(
            new OrderCreatedDomainEvent(
                order.Id,
                customerId,
                0));

        return order;
    }

    #endregion

    #region Item Management

    /// <summary>
    /// Adds a product to the order.
    /// If the product already exists,
    /// its quantity is increased.
    /// </summary>
    public void AddItem(
        Guid productId,
        string productName,
        Money unitPrice,
        int quantity)
    {
        EnsureCanModify();

        ArgumentException.ThrowIfNullOrWhiteSpace(productName);

        if (quantity <= 0)
        {
            throw new BusinessRuleViolationException(
                "Quantity must be greater than zero.");
        }

        var existingItem =
            _items.FirstOrDefault(x => x.ProductId == productId);

        if (existingItem is not null)
        {
            existingItem.IncreaseQuantity(quantity);
        }
        else
        {
            var item = OrderItem.Create(
                Guid.NewGuid(),
                productId,
                productName,
                unitPrice,
                quantity);

            _items.Add(item);
        }

        RecalculateTotal();
    }

    /// <summary>
    /// Removes a product from the order.
    /// </summary>
    public void RemoveItem(Guid productId)
    {
        EnsureCanModify();

        var item =
            _items.FirstOrDefault(x => x.ProductId == productId);

        if (item is null)
            return;

        _items.Remove(item);

        RecalculateTotal();
    }

    /// <summary>
    /// Changes the quantity of an existing order item.
    /// </summary>
    public void ChangeQuantity(
        Guid productId,
        int quantity)
    {
        EnsureCanModify();

        var item =
            FindOrderItem(productId);

        item.SetQuantity(quantity);

        RecalculateTotal();
    }

    #endregion

    #region State Management

    /// <summary>
    /// Confirms the order.
    /// </summary>
    public void Confirm()
    {
        if (_items.Count == 0)
        {
            throw new BusinessRuleViolationException(
                "Cannot confirm an empty order.");
        }

        EnsureStatus(
            OrderStatus.Pending,
            "Confirm");

        Status = OrderStatus.Confirmed;

        AddDomainEvent(
            new OrderConfirmedDomainEvent(Id));
    }

    /// <summary>
    /// Cancels the order.
    /// </summary>
    public void Cancel(string reason)
    {
        if (Status is OrderStatus.Shipped
            or OrderStatus.Delivered)
        {
            throw new InvalidOrderStateException(
                Status.ToString(),
                "Cancel");
        }

        Status = OrderStatus.Cancelled;

        AddDomainEvent(
            new OrderCancelledDomainEvent(
                Id,
                reason));
    }

    /// <summary>
    /// Ships the order.
    /// </summary>
    public void Ship()
    {
        EnsureStatus(
            OrderStatus.Confirmed,
            "Ship");

        Status = OrderStatus.Shipped;

        AddDomainEvent(
            new OrderShippedDomainEvent(Id));
    }

    /// <summary>
    /// Marks the order as delivered.
    /// </summary>
    public void Deliver()
    {
        EnsureStatus(
            OrderStatus.Shipped,
            "Deliver");

        Status = OrderStatus.Delivered;

        AddDomainEvent(
            new OrderDeliveredDomainEvent(Id));
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Calculates the order total.
    /// </summary>
    private void RecalculateTotal()
    {
        var total = Money.Zero("USD");

        foreach (var item in _items)
        {
            total += item.LineTotal;
        }

        TotalAmount = total;
    }

    /// <summary>
    /// Finds an order item.
    /// </summary>
    private OrderItem FindOrderItem(Guid productId)
    {
        return _items.FirstOrDefault(x => x.ProductId == productId)
            ?? throw new BusinessRuleViolationException(
                "The specified product does not exist in this order.");
    }

    /// <summary>
    /// Ensures the order can still be modified.
    /// </summary>
    private void EnsureCanModify()
    {
        if (Status is OrderStatus.Shipped
            or OrderStatus.Delivered
            or OrderStatus.Cancelled)
        {
            throw new InvalidOrderStateException(
                Status.ToString(),
                "Modify Order");
        }
    }

    /// <summary>
    /// Ensures the order is in the expected state.
    /// </summary>
    private void EnsureStatus(
        OrderStatus expectedStatus,
        string action)
    {
        if (Status != expectedStatus)
        {
            throw new InvalidOrderStateException(
                Status.ToString(),
                action);
        }
    }

    #endregion
}
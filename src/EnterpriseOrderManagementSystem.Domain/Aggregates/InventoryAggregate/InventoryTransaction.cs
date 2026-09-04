using EnterpriseOrderManagementSystem.Domain.Common.Base;
using EnterpriseOrderManagementSystem.Domain.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Enums;

namespace EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate;

/// <summary>
/// Represents a single inventory state transition.
///
/// InventoryTransaction is an entity owned by the
/// Inventory aggregate root.
///
/// The transaction provides an immutable-style historical
/// record of inventory movements, including:
///
/// - Available quantity before the movement
/// - Available quantity after the movement
/// - Reserved quantity before the movement
/// - Reserved quantity after the movement
/// - Movement type
/// - Business reference
/// - Business reason
/// - Timestamp
///
/// Transactions should only be created by the Inventory aggregate.
/// </summary>
public sealed class InventoryTransaction : Entity<Guid>
{
    #region Constructors

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    private InventoryTransaction()
    {
    }

    /// <summary>
    /// Initializes a new inventory transaction.
    /// </summary>
    private InventoryTransaction(
        Guid id,
        InventoryMovementType movementType,
        int quantity,
        int availableQuantityBefore,
        int availableQuantityAfter,
        int reservedQuantityBefore,
        int reservedQuantityAfter,
        string reference,
        string reason,
        DateTime occurredOnUtc)
    {
        Id = id;

        MovementType = movementType;

        Quantity = quantity;

        AvailableQuantityBefore = availableQuantityBefore;

        AvailableQuantityAfter = availableQuantityAfter;

        ReservedQuantityBefore = reservedQuantityBefore;

        ReservedQuantityAfter = reservedQuantityAfter;

        Reference = reference;

        Reason = reason;

        OccurredOnUtc = occurredOnUtc;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the type of inventory movement.
    /// </summary>
    public InventoryMovementType MovementType { get; private set; }

    /// <summary>
    /// Gets the quantity affected by the movement.
    ///
    /// For example:
    /// - Receive 100
    /// - Reserve 20
    /// - Release 10
    /// - Commit 20
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// Gets the available quantity before
    /// the inventory movement.
    /// </summary>
    public int AvailableQuantityBefore { get; private set; }

    /// <summary>
    /// Gets the available quantity after
    /// the inventory movement.
    /// </summary>
    public int AvailableQuantityAfter { get; private set; }

    /// <summary>
    /// Gets the reserved quantity before
    /// the inventory movement.
    /// </summary>
    public int ReservedQuantityBefore { get; private set; }

    /// <summary>
    /// Gets the reserved quantity after
    /// the inventory movement.
    /// </summary>
    public int ReservedQuantityAfter { get; private set; }

    /// <summary>
    /// Gets the external business reference associated
    /// with this transaction.
    ///
    /// Examples:
    /// - Purchase Order Number
    /// - Order Number
    /// - Shipment Number
    /// - Stock Adjustment Number
    /// </summary>
    public string Reference { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the business reason for the movement.
    /// </summary>
    public string Reason { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the UTC timestamp when the transaction occurred.
    /// </summary>
    public DateTime OccurredOnUtc { get; private set; }

    /// <summary>
    /// Gets the inventory that owns this transaction.
    /// </summary>
    public Inventory Inventory { get; private set; } = default!;

    #endregion

    #region Computed Properties

    /// <summary>
    /// Gets the total inventory before
    /// the movement.
    /// </summary>
    public int TotalQuantityBefore =>
        AvailableQuantityBefore + ReservedQuantityBefore;

    /// <summary>
    /// Gets the total inventory after
    /// the movement.
    /// </summary>
    public int TotalQuantityAfter =>
        AvailableQuantityAfter + ReservedQuantityAfter;

    public bool IncreasedInventory =>
    TotalQuantityAfter > TotalQuantityBefore;

    public bool DecreasedInventory =>
        TotalQuantityAfter < TotalQuantityBefore;

    #endregion

    #region Factory Methods

    /// <summary>
    /// Creates a new inventory transaction.
    /// </summary>
    public static InventoryTransaction Create(        
        InventoryMovementType movementType,
        int quantity,
        int availableQuantityBefore,
        int availableQuantityAfter,
        int reservedQuantityBefore,
        int reservedQuantityAfter,
        string reference,
        string reason)
    {
        #region Validation

        ValidateQuantity(quantity);

        ValidateReference(reference.Trim());

        ValidateReason(reason.Trim());

        if (availableQuantityBefore < 0)
        {
            throw new BusinessRuleViolationException(
                "Available quantity before transaction cannot be negative.");
        }

        if (availableQuantityAfter < 0)
        {
            throw new BusinessRuleViolationException(
                "Available quantity after transaction cannot be negative.");
        }

        if (reservedQuantityBefore < 0)
        {
            throw new BusinessRuleViolationException(
                "Reserved quantity before transaction cannot be negative.");
        }

        if (reservedQuantityAfter < 0)
        {
            throw new BusinessRuleViolationException(
                "Reserved quantity after transaction cannot be negative.");
        }

        if (availableQuantityBefore == availableQuantityAfter &&
            reservedQuantityBefore == reservedQuantityAfter)
        {
            throw new BusinessRuleViolationException(
                "Inventory transaction must change inventory state.");
        }

        #endregion

        #region Create Entity

        return new InventoryTransaction(
            id: Guid.NewGuid(),
            movementType: movementType,
            quantity: quantity,
            availableQuantityBefore: availableQuantityBefore,
            availableQuantityAfter: availableQuantityAfter,
            reservedQuantityBefore: reservedQuantityBefore,
            reservedQuantityAfter: reservedQuantityAfter,
            reference: reference.Trim(),
            reason: reason.Trim(),
            occurredOnUtc: DateTime.UtcNow);

        #endregion
    }

    #endregion

    #region Validation Methods

    /// <summary>
    /// Validates that the transaction quantity is greater than zero.
    /// </summary>
    /// <param name="quantity">
    /// The transaction quantity.
    /// </param>
    private static void ValidateQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new BusinessRuleViolationException(
                "Transaction quantity must be greater than zero.");
        }
    }

    /// <summary>
    /// Validates the business reference associated with the
    /// inventory transaction.
    /// </summary>
    /// <param name="reference">
    /// Business reference.
    /// </param>
    private static void ValidateReference(string reference)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reference);
    }

    /// <summary>
    /// Validates the business reason associated with the
    /// inventory transaction.
    /// </summary>
    /// <param name="reason">
    /// Business reason.
    /// </param>
    private static void ValidateReason(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
    }

    #endregion
}
using EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate.Events;
using EnterpriseOrderManagementSystem.Domain.Common.Base;
using EnterpriseOrderManagementSystem.Domain.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Enums;

namespace EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate;

/// <summary>
/// Represents the inventory maintained for a specific product
/// within a warehouse.
///
/// The Inventory aggregate is responsible for:
///
/// • Tracking available stock
/// • Tracking reserved stock
/// • Enforcing warehouse capacity
/// • Managing reorder thresholds
/// • Recording inventory transactions
/// • Raising domain events
/// • Protecting all inventory business rules
///
/// This aggregate is the single source of truth for inventory
/// operations within the domain.
/// </summary>
public sealed class Inventory : AggregateRoot<Guid>
{
    #region Private Fields

    /// <summary>
    /// Stores the inventory transaction history.
    /// </summary>
    private readonly List<InventoryTransaction> _transactions = [];

    #endregion

    #region Constructors

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    private Inventory()
    {
    }

    /// <summary>
    /// Creates a new inventory aggregate.
    /// Use the Create() factory method instead.
    /// </summary>
    private Inventory(
        Guid inventoryId,
        Guid productId,
        Guid warehouseId,
        int reorderLevel,
        int maximumQuantity)
    {
        Id = inventoryId;

        ProductId = productId;
        WarehouseId = warehouseId;

        AvailableQuantity = 0;
        ReservedQuantity = 0;

        ReorderLevel = reorderLevel;
        MaximumQuantity = maximumQuantity;

        IsDiscontinued = false;

        LastStockMovementUtc = DateTime.UtcNow;
    }

    #endregion

    #region Properties

    #region Identity

    /// <summary>
    /// Gets the associated product identifier.
    /// </summary>
    public Guid ProductId { get; private set; }

    /// <summary>
    /// Gets the warehouse identifier.
    /// </summary>
    public Guid WarehouseId { get; private set; }

    #endregion

    #region Stock Information

    /// <summary>
    /// Gets the quantity currently available
    /// for reservation or sale.
    /// </summary>
    public int AvailableQuantity { get; private set; }

    /// <summary>
    /// Gets the quantity currently reserved
    /// by pending orders.
    /// </summary>
    public int ReservedQuantity { get; private set; }

    /// <summary>
    /// Gets the quantity at which replenishment
    /// should be triggered.
    /// </summary>
    public int ReorderLevel { get; private set; }

    /// <summary>
    /// Gets the maximum warehouse capacity
    /// for this product.
    /// </summary>
    public int MaximumQuantity { get; private set; }

    #endregion

    #region Status Information

    /// <summary>
    /// Gets a value indicating whether
    /// this inventory is discontinued.
    /// </summary>
    public bool IsDiscontinued { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp of the last
    /// inventory movement.
    /// </summary>
    public DateTime LastStockMovementUtc { get; private set; }

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Gets the inventory transaction history.
    ///
    /// The collection is exposed as read-only to prevent
    /// external modification of the aggregate state.
    /// </summary>
    public IReadOnlyCollection<InventoryTransaction> Transactions =>
        _transactions.AsReadOnly();

    #endregion

    #region Computed Properties

    /// <summary>
    /// Gets the total inventory currently
    /// owned by the warehouse.
    ///
    /// Total Quantity =
    /// Available + Reserved
    /// </summary>
    public int TotalQuantity =>
        AvailableQuantity + ReservedQuantity;

    /// <summary>
    /// Gets the remaining warehouse capacity.
    /// </summary>
    public int RemainingCapacity =>
        MaximumQuantity - TotalQuantity;

    /// <summary>
    /// Gets a value indicating whether
    /// additional inventory can be received.
    /// </summary>
    public bool HasRemainingCapacity =>
        RemainingCapacity > 0;

    /// <summary>
    /// Gets a value indicating whether
    /// inventory has reached the reorder level.
    /// </summary>
    public bool NeedsReorder =>
        !IsDiscontinued &&
        AvailableQuantity <= ReorderLevel;

    /// <summary>
    /// Gets a value indicating whether
    /// inventory is completely depleted.
    /// </summary>
    public bool IsOutOfStock =>
        AvailableQuantity == 0;

    /// <summary>
    /// Gets the current inventory status.
    /// </summary>
    public InventoryStatus Status
    {
        get
        {
            if (IsDiscontinued)
            {
                return InventoryStatus.Discontinued;
            }

            if (AvailableQuantity == 0)
            {
                return InventoryStatus.OutOfStock;
            }

            if (AvailableQuantity <= ReorderLevel)
            {
                return InventoryStatus.LowStock;
            }

            return InventoryStatus.InStock;
        }
    }

    #endregion

    #endregion

    #region Factory Methods

    /// <summary>
    /// Creates a new inventory record.
    /// </summary>
    /// <param name="inventoryId">
    /// Inventory identifier.
    /// </param>
    /// <param name="productId">
    /// Product identifier.
    /// </param>
    /// <param name="warehouseId">
    /// Warehouse identifier.
    /// </param>
    /// <param name="reorderLevel">
    /// Minimum quantity before replenishment.
    /// </param>
    /// <param name="maximumQuantity">
    /// Maximum warehouse capacity.
    /// </param>
    /// <returns>
    /// A new Inventory aggregate.
    /// </returns>
    public static Inventory Create(
        Guid inventoryId,
        Guid productId,
        Guid warehouseId,
        int reorderLevel,
        int maximumQuantity)
    {
        if (inventoryId == Guid.Empty)
            throw new BusinessRuleViolationException(
                "Inventory Id cannot be empty.");

        if (productId == Guid.Empty)
            throw new BusinessRuleViolationException(
                "Product Id cannot be empty.");

        if (warehouseId == Guid.Empty)
            throw new BusinessRuleViolationException(
                "Warehouse Id cannot be empty.");

        if (reorderLevel < 0)
            throw new BusinessRuleViolationException(
                "Reorder level cannot be negative.");

        if (maximumQuantity <= 0)
            throw new BusinessRuleViolationException(
                "Maximum quantity must be greater than zero.");

        if (reorderLevel > maximumQuantity)
            throw new BusinessRuleViolationException(
                "Reorder level cannot exceed maximum quantity.");

        EnsureValidReorderLevel(
            reorderLevel,
            maximumQuantity);

        EnsureValidMaximumQuantity(
            maximumQuantity,
            reorderLevel);

        var inventory = new Inventory(
            inventoryId,
            productId,
            warehouseId,
            reorderLevel,
            maximumQuantity);

        inventory.RaiseInventoryCreatedEvent();

        return inventory;
    }

    #endregion

    #region Stock Operations

    /// <summary>
    /// Receives stock into the warehouse.
    /// </summary>
    /// <param name="quantity">
    /// Quantity received.
    /// </param>
    /// <param name="reference">
    /// Business reference (e.g. Purchase Order Number).
    /// </param>
    /// <param name="reason">
    /// Business reason.
    /// </param>
    public void ReceiveStock(
        int quantity,
        string reference,
        string reason)
    {
        ValidatePositiveQuantity(quantity);

        EnsureInventoryIsActive();

        EnsureWarehouseCapacity(quantity);

        ValidateReferenceAndReason(
               reference,
               reason);

        var previousStatus = Status;

        var availableBefore = AvailableQuantity;

        var reservedBefore = ReservedQuantity;

        AvailableQuantity += quantity;

        UpdateLastMovement();

        RecordInventoryTransaction(
            InventoryMovementType.Receive,
            quantity,
            availableBefore,
            AvailableQuantity,
            reservedBefore,
            ReservedQuantity,
            reference,
            reason);

        RaiseStockReceivedEvent(
            quantity,
            reference,
            reason);

        RaiseInventoryStatusEvents(previousStatus);
    }

    /// <summary>
    /// Reserves inventory for an order.
    /// </summary>
    public void ReserveStock(
        int quantity,
        string reference,
        string reason)
    {
        ValidatePositiveQuantity(quantity);

        EnsureInventoryIsActive();

        EnsureAvailableStock(quantity);

        ValidateReferenceAndReason(
              reference,
              reason);

        var previousStatus = Status;

        var availableBefore = AvailableQuantity;
        var reservedBefore = ReservedQuantity;

        AvailableQuantity -= quantity;
        ReservedQuantity += quantity;

        UpdateLastMovement();

        RecordInventoryTransaction(
            InventoryMovementType.Reserve,
            quantity,
            availableBefore,
            AvailableQuantity,
            reservedBefore,
            ReservedQuantity,
            reference,
            reason);

        RaiseStockReservedEvent(
            quantity,
            reference,
            reason);

        RaiseInventoryStatusEvents(previousStatus);
    }

    /// <summary>
    /// Releases previously reserved stock.
    /// </summary>
    public void ReleaseReservedStock(
        int quantity,
        string reference,
        string reason)
    {
        ValidatePositiveQuantity(quantity);

        EnsureReservedStock(quantity);

        ValidateReferenceAndReason(
                reference,
                reason);

        var previousStatus = Status;

        var availableBefore = AvailableQuantity;
        var reservedBefore = ReservedQuantity;

        ReservedQuantity -= quantity;
        AvailableQuantity += quantity;

        UpdateLastMovement();

        RecordInventoryTransaction(
            InventoryMovementType.Release,
            quantity,
            availableBefore,
            AvailableQuantity,
            reservedBefore,
            ReservedQuantity,
            reference,
            reason);

        RaiseStockReleasedEvent(quantity, reference, reason);

        RaiseInventoryStatusEvents(previousStatus);
    }

    /// <summary>
    /// Commits reserved inventory after shipment.
    /// </summary>
    public void CommitReservation(
        int quantity,
        string reference,
        string reason)
    {
        ValidatePositiveQuantity(quantity);

        EnsureReservedStock(quantity);

        ValidateReferenceAndReason(
                      reference,
                      reason);

        var previousStatus = Status;

        var availableBefore = AvailableQuantity;
        var reservedBefore = ReservedQuantity;

        ReservedQuantity -= quantity;
        
        UpdateLastMovement();

        RecordInventoryTransaction(
            InventoryMovementType.Commit,
            quantity,
            availableBefore,
            AvailableQuantity,
            reservedBefore,
            ReservedQuantity,
            reference,
            reason);

        RaiseStockCommittedEvent(quantity, reference, reason);

        RaiseInventoryStatusEvents(previousStatus);
    }

    /// <summary>
    /// Adjusts the available inventory quantity.
    ///
    /// This operation should only be used for manual stock corrections,
    /// damaged items, inventory recounts, or system reconciliation.
    /// It should never be used for normal order processing.
    /// </summary>
    /// <param name="newAvailableQuantity">
    /// The corrected available quantity.
    /// </param>    
    /// <param name="reason">
    /// Business reason for the adjustment.
    /// </param>
    public void AdjustStock(
     int newAvailableQuantity,
     string reference,
     string reason)
    {        
        if (newAvailableQuantity < 0)
        {
            throw new BusinessRuleViolationException(
                "Available quantity cannot be negative.");
        }

        if (newAvailableQuantity + ReservedQuantity > MaximumQuantity)
        {
            throw new BusinessRuleViolationException(
                "Adjusted quantity exceeds maximum warehouse capacity.");
        }

        ValidateReferenceAndReason(
              reference,
              reason);

        var previousStatus = Status;

        var availableBefore = AvailableQuantity;
        var reservedBefore = ReservedQuantity;

        var adjustmentQuantity = Math.Abs(newAvailableQuantity - AvailableQuantity);

        AvailableQuantity = newAvailableQuantity;

        UpdateLastMovement();

        RecordInventoryTransaction(
            InventoryMovementType.Adjustment,
            adjustmentQuantity,
            availableBefore,
            AvailableQuantity,
            reservedBefore,
            ReservedQuantity,
            reference,
            reason);

        RaiseStockAdjustedEvent(
             availableBefore,
             AvailableQuantity,
             reference,
             reason);

        RaiseInventoryStatusEvents(previousStatus);
    }

    #endregion

    #region Administration Operations

    /// <summary>
    /// Updates the inventory reorder level.
    ///
    /// The reorder level determines when replenishment
    /// should be initiated.
    /// </summary>
    /// <param name="reorderLevel">
    /// New reorder level.
    /// </param>
    public void UpdateReorderLevel(int reorderLevel)
    {       
        EnsureInventoryIsActive();

        var previousStatus = Status;

        EnsureValidReorderLevel(reorderLevel);

        ReorderLevel = reorderLevel;

        RaiseInventoryStatusEvents(previousStatus);
    }

    /// <summary>
    /// Updates the maximum warehouse capacity.
    ///
    /// The new capacity cannot be less than the current
    /// total inventory.
    /// </summary>
    /// <param name="maximumQuantity">
    /// New maximum warehouse capacity.
    /// </param>
    public void UpdateMaximumQuantity(int maximumQuantity)
    {
        EnsureInventoryIsActive();

        EnsureValidMaximumQuantity(maximumQuantity);

        MaximumQuantity = maximumQuantity;
    }

    /// <summary>
    /// Marks the inventory as discontinued.
    ///
    /// Discontinued inventory cannot receive, reserve,
    /// release, commit, or adjust stock.
    /// </summary>
    /// <param name="reason">
    /// Business reason for discontinuation.
    /// </param>
    public void Discontinue(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        if (IsDiscontinued)
        {
            throw new BusinessRuleViolationException(
                "Inventory is already discontinued.");
        }

        IsDiscontinued = true;

        UpdateLastMovement();

        RaiseInventoryDiscontinuedEvent(reason);
    }

    /// <summary>
    /// Reactivates a discontinued inventory.
    ///
    /// This operation should only be performed
    /// through an administrative workflow.
    /// </summary>
    public void Reactivate()
    {
        if (!IsDiscontinued)
        {
            throw new BusinessRuleViolationException(
                "Inventory is already active.");
        }

        var previousStatus = Status;

        IsDiscontinued = false;

        UpdateLastMovement();

        RaiseInventoryReactivatedEvent();

        RaiseInventoryStatusEvents(previousStatus);
    }

    #endregion

    #region Transaction Methods

    /// <summary>
    /// Records an inventory transaction representing a stock movement.
    ///
    /// Inventory transactions provide an immutable audit trail of
    /// inventory quantity changes.
    ///
    /// This method should only be called after the aggregate state
    /// has been successfully changed.
    /// </summary>
    /// <param name="movementType">
    /// Type of inventory movement.
    /// </param>
    /// <param name="quantity">
    /// Quantity involved in the movement.
    /// </param>
    /// <param name="availableQuantityBefore">
    /// Available quantity before the operation.
    /// </param>
    /// <param name="availableQuantityAfter">
    /// Available quantity after the operation.
    /// </param>
    /// <param name="reservedQuantityBefore">
    /// Reserved quantity before the operation.
    /// </param>
    /// <param name="reservedQuantityAfter">
    /// Reserved quantity after the operation.
    /// </param>
    /// <param name="reference">
    /// Business reference associated with the movement.
    /// </param>
    /// <param name="reason">
    /// Business reason for the movement.
    /// </param>
    private void RecordInventoryTransaction(
        InventoryMovementType movementType,
        int quantity,
        int availableQuantityBefore,
        int availableQuantityAfter,
        int reservedQuantityBefore,
        int reservedQuantityAfter,
        string reference,
        string reason)
    {
        var transaction = InventoryTransaction.Create(                
            movementType,
            quantity,
            availableQuantityBefore,
            availableQuantityAfter,
            reservedQuantityBefore,
            reservedQuantityAfter,
            reference,
            reason);

        _transactions.Add(transaction);
    }

    #endregion

    #region Domain Event Methods

    /// <summary>
    /// Raises an event indicating that inventory was created.
    /// </summary>
    private void RaiseInventoryCreatedEvent()
    {
        AddDomainEvent(
            new InventoryCreatedDomainEvent(
                Id,
                ProductId,
                WarehouseId));
    }

    /// <summary>
    /// Raises an event indicating that stock was received.
    /// </summary>
    private void RaiseStockReceivedEvent(
        int quantity,
        string reference,
        string reason)
    {
        AddDomainEvent(
            new StockReceivedDomainEvent(
                Id,
                ProductId,
                WarehouseId,
                quantity,
                reference,
                reason));
    }

    /// <summary>
    /// Raises an event indicating that stock was reserved.
    /// </summary>
    private void RaiseStockReservedEvent(
        int quantity,
        string reference,
        string reason)
    {
        AddDomainEvent(
            new StockReservedDomainEvent(
                Id,
                ProductId,
                WarehouseId,
                quantity,
                reference,
                reason));
    }

    /// <summary>
    /// Raises an event indicating that reserved stock was released.
    /// </summary>
    private void RaiseStockReleasedEvent(
        int quantity,
        string reference,
        string reason)
    {
        AddDomainEvent(
            new StockReleasedDomainEvent(
                Id,
                ProductId,
                WarehouseId,
                quantity,
                reference,
                reason));
    }

    /// <summary>
    /// Raises an event indicating that reserved stock was committed.
    /// </summary>
    private void RaiseStockCommittedEvent(
        int quantity,
        string reference,
        string reason)
    {
        AddDomainEvent(
            new StockCommittedDomainEvent(
                Id,
                ProductId,
                WarehouseId,
                quantity,
                reference,
                reason));
    }

    /// <summary>
    /// Raises an event indicating that inventory was manually adjusted.
    /// </summary>
    private void RaiseStockAdjustedEvent(
        int previousQuantity,
        int currentQuantity,
        string reference,
        string reason)
    {
        AddDomainEvent(
            new StockAdjustedDomainEvent(
                Id,
                ProductId,
                WarehouseId,
                previousQuantity,
                currentQuantity,
                reference,
                reason));
    }

    /// <summary>
    /// Raises an event indicating that inventory has been discontinued.
    /// </summary>
    private void RaiseInventoryDiscontinuedEvent(
        string reason)
    {
        AddDomainEvent(
            new InventoryDiscontinuedDomainEvent(
                Id,
                ProductId,
                WarehouseId,
                reason));
    }

    /// <summary>
    /// Raises an event indicating that discontinued inventory
    /// has been reactivated.
    /// </summary>
    private void RaiseInventoryReactivatedEvent()
    {
        AddDomainEvent(
            new InventoryReactivatedDomainEvent(
                Id,
                ProductId,
                WarehouseId));
    }

    #endregion

    #region Inventory Status Events

    /// <summary>
    /// Evaluates whether the inventory status changed
    /// and raises the appropriate domain event.
    ///
    /// Status events are raised only when the aggregate
    /// transitions into a new status.
    /// </summary>
    /// <param name="previousStatus">
    /// Inventory status before the operation.
    /// </param>
    private void RaiseInventoryStatusEvents(
        InventoryStatus previousStatus)
    {
        var currentStatus = Status;

        if (previousStatus == currentStatus)
        {
            return;
        }

        switch (currentStatus)
        {
            case InventoryStatus.LowStock:

                RaiseLowStockDetectedEvent();

                break;

            case InventoryStatus.OutOfStock:

                RaiseOutOfStockEvent();

                break;

            case InventoryStatus.Discontinued:

                // Discontinuation has its own dedicated
                // domain event and is handled separately.
                break;

            case InventoryStatus.InStock:

                // No event is currently required when
                // inventory returns to normal stock levels.
                break;

            default:

                throw new BusinessRuleViolationException(
                    $"Unsupported inventory status: {currentStatus}.");
        }
    }

    /// <summary>
    /// Raises an event indicating that inventory
    /// has reached the low-stock threshold.
    /// </summary>
    private void RaiseLowStockDetectedEvent()
    {
        AddDomainEvent(
            new LowStockDetectedDomainEvent(
                Id,
                ProductId,
                WarehouseId,
                AvailableQuantity,
                ReorderLevel));
    }

    /// <summary>
    /// Raises an event indicating that inventory
    /// is completely out of stock.
    /// </summary>
    private void RaiseOutOfStockEvent()
    {
        AddDomainEvent(
            new OutOfStockDomainEvent(
                Id,
                ProductId,
                WarehouseId));
    }

    #endregion

    #region Validation Methods   

    /// <summary>
    /// Ensures the supplied quantity is greater than zero.
    /// </summary>
    private static void ValidatePositiveQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new BusinessRuleViolationException(
                "Quantity must be greater than zero.");
        }
    }   

    /// <summary>
    /// Validates that a business reference and reason
    /// have been provided.
    /// </summary>
    private static void ValidateReferenceAndReason(
        string reference,
        string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            reference);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            reason);
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Ensures that the inventory is currently active.
    /// </summary>
    private void EnsureInventoryIsActive()
    {
        if (IsDiscontinued)
        {
            throw new BusinessRuleViolationException(
                "The inventory is discontinued and cannot be modified.");
        }
    }

    /// <summary>
    /// Ensures that sufficient available stock exists
    /// for the requested operation.
    /// </summary>
    private void EnsureAvailableStock(
        int quantity)
    {
        if (AvailableQuantity < quantity)
        {
            throw new BusinessRuleViolationException(
                $"Insufficient available stock. " +
                $"Requested: {quantity}. " +
                $"Available: {AvailableQuantity}.");
        }
    }

    /// <summary>
    /// Ensures that sufficient reserved stock exists
    /// for the requested operation.
    /// </summary>
    private void EnsureReservedStock(
        int quantity)
    {
        if (ReservedQuantity < quantity)
        {
            throw new BusinessRuleViolationException(
                $"Insufficient reserved stock. " +
                $"Requested: {quantity}. " +
                $"Reserved: {ReservedQuantity}.");
        }
    }

    /// <summary>
    /// Ensures that receiving the specified quantity
    /// does not exceed warehouse capacity.
    /// </summary>
    private void EnsureWarehouseCapacity(
        int quantity)
    {
        if (TotalQuantity + quantity > MaximumQuantity)
        {
            throw new BusinessRuleViolationException(
                $"Warehouse capacity exceeded. " +
                $"Maximum: {MaximumQuantity}. " +
                $"Current: {TotalQuantity}. " +
                $"Requested: {quantity}.");
        }
    }

    /// <summary>
    /// Validates the reorder level.
    /// </summary>
    /// <param name="reorderLevel"></param>
    /// <param name="maximumQuantity"></param>
    /// <exception cref="BusinessRuleViolationException"></exception>
    private static void EnsureValidReorderLevel(int reorderLevel, int maximumQuantity)
    {
        if (reorderLevel < 0)
        {
            throw new BusinessRuleViolationException(
                "Reorder level cannot be negative.");
        }

        if (reorderLevel > maximumQuantity)
        {
            throw new BusinessRuleViolationException(
                 "Reorder level cannot exceed the maximum warehouse capacity.");
        }
    }

    /// <summary>
    /// Ensures that the specified reorder level is valid.
    ///
    /// The reorder level:
    /// - Cannot be negative.
    /// - Cannot exceed the maximum inventory capacity.
    /// </summary>
    /// <param name="reorderLevel">
    /// The reorder level to validate.
    /// </param>
    private void EnsureValidReorderLevel(int reorderLevel)
    {
        EnsureValidReorderLevel(
            reorderLevel,
            MaximumQuantity);
    }

    /// <summary>
    /// Validates warehouse capacity.
    /// </summary>
    private static void EnsureValidMaximumQuantity(int maximumQuantity, int reorderLevel)
    {
        if (maximumQuantity <= 0)
        {
            throw new BusinessRuleViolationException(
                "Maximum quantity must be greater than zero.");
        }

        if (reorderLevel > maximumQuantity)
        {
            throw new BusinessRuleViolationException(
                "Maximum quantity cannot be less than the reorder level.");
        }
    }

    /// <summary>
    /// Ensures that the specified maximum quantity is valid.
    ///
    /// The maximum quantity:
    /// - Must be greater than zero.
    /// - Cannot be less than the current total inventory.
    /// - Cannot be less than the reorder level.
    /// </summary>
    /// <param name="maximumQuantity">
    /// The maximum quantity to validate.
    /// </param>
    private void EnsureValidMaximumQuantity(int maximumQuantity)
    {
        if (maximumQuantity <= 0)
        {
            throw new BusinessRuleViolationException(
                "Maximum quantity must be greater than zero.");
        }

        if (maximumQuantity < TotalQuantity)
        {
            throw new BusinessRuleViolationException(
                "Maximum quantity cannot be less than the current inventory.");
        }

        if (maximumQuantity < ReorderLevel)
        {
            throw new BusinessRuleViolationException(
                "Maximum quantity cannot be less than the reorder level.");
        }
    }

    /// <summary>
    /// Updates the timestamp of the most recent
    /// inventory movement.
    /// </summary>
    private void UpdateLastMovement()
    {
        LastStockMovementUtc = DateTime.UtcNow;
    }

    #endregion


}
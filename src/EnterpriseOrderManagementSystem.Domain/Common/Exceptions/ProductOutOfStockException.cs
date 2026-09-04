namespace EnterpriseOrderManagementSystem.Domain.Common.Exceptions;

/// <summary>
/// Thrown when requested inventory is unavailable.
/// </summary>
public sealed class ProductOutOfStockException
    : DomainException
{
    public ProductOutOfStockException(
        Guid productId,
        int requestedQuantity,
        int availableQuantity)
        : base(
            $"Product '{productId}' has only " +
            $"{availableQuantity} items available. " +
            $"Requested: {requestedQuantity}.")
    {
    }
}
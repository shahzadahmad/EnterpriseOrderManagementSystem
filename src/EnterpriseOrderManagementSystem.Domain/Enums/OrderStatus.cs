namespace EnterpriseOrderManagementSystem.Domain.Enums;

/// <summary>
/// Represents the lifecycle of an order.
/// </summary>
public enum OrderStatus
{
    Pending = 1,

    Confirmed = 2,

    Processing = 3,

    Shipped = 4,

    Delivered = 5,

    Cancelled = 6
}
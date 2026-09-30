namespace EnterpriseOrderManagementSystem.Application.DTOs.Orders;

public sealed record OrderItemDto(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    string Currency,
    int Quantity,
    decimal LineTotal);

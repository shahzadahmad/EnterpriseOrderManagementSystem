using EnterpriseOrderManagementSystem.Domain.Enums;

namespace EnterpriseOrderManagementSystem.Application.DTOs.Orders;

public sealed record OrderDto(
    Guid Id,
    Guid CustomerId,
    OrderStatus Status,
    decimal TotalAmount,
    string Currency,
    IReadOnlyCollection<OrderItemDto> Items);

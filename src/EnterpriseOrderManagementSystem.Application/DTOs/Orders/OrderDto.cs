using EnterpriseOrderManagementSystem.Domain.Enums;

namespace EnterpriseOrderManagementSystem.Application.DTOs.Orders;

public sealed record OrderDto(
    Guid Id,
    Guid CustomerId,
    string OrderNumber,
    OrderStatus Status,
    decimal TotalAmount,
    string Currency,
    DateTime CreatedOnUtc,
    DateTime? CancelledOnUtc,
    string? CancellationReason,
    IReadOnlyCollection<OrderItemDto> Items);

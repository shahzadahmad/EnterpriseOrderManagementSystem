using EnterpriseOrderManagementSystem.Application.DTOs.Orders;
using EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate;

namespace EnterpriseOrderManagementSystem.Application.Mapping;

public static class OrderMapping
{
    public static OrderDto ToDto(this Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        var items = order.Items
            .Select(item => item.ToDto())
            .ToList();

        return new OrderDto(
            order.Id,
            order.CustomerId,
            order.Status,
            order.TotalAmount.Amount,
            order.TotalAmount.Currency,
            items);
    }

    private static OrderItemDto ToDto(this OrderItem item)
    {
        return new OrderItemDto(
            item.ProductId,
            item.ProductName,
            item.UnitPrice.Amount,
            item.UnitPrice.Currency,
            item.Quantity,
            item.LineTotal.Amount);
    }
}
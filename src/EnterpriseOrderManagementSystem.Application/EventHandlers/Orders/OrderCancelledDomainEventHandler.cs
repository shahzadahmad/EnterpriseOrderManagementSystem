using EnterpriseOrderManagementSystem.Application.Abstractions.Services;
using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Application.Common.Interfaces;
using EnterpriseOrderManagementSystem.Domain.Aggregates.CustomerAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate.Events;
using EnterpriseOrderManagementSystem.Domain.Repositories;

namespace EnterpriseOrderManagementSystem.Application.EventHandlers.Orders;

/// <summary>Handles application-level notification for OrderCancelledDomainEvent.</summary>
public sealed class OrderCancelledDomainEventHandler : IDomainEventHandler<OrderCancelledDomainEvent>
{
    private readonly IOrderRepository _orders;
    private readonly ICustomerRepository _customers;
    private readonly IEmailService _emailService;

    public OrderCancelledDomainEventHandler(IOrderRepository orders, ICustomerRepository customers, IEmailService emailService)
    {
        _orders = orders;
        _customers = customers;
        _emailService = emailService;
    }

    public async Task HandleAsync(OrderCancelledDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var order = await _orders.GetByIdAsync(domainEvent.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), domainEvent.OrderId);
        var customer = await _customers.GetByIdAsync(order.CustomerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), order.CustomerId);

        await _emailService.SendAsync(
            customer.Email.ToString(),
            "Order cancelled",
            $"Your order {domainEvent.OrderId} has been cancelled. Reason: {domainEvent.Reason}",
            cancellationToken);
    }
}

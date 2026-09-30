using EnterpriseOrderManagementSystem.Application.Abstractions.Services;
using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Application.Common.Interfaces;
using EnterpriseOrderManagementSystem.Domain.Aggregates.CustomerAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.PaymentAggregate.Events;
using EnterpriseOrderManagementSystem.Domain.Repositories;

namespace EnterpriseOrderManagementSystem.Application.EventHandlers.Payments;

/// <summary>Handles application-level notification for PaymentCapturedDomainEvent.</summary>
public sealed class PaymentCapturedDomainEventHandler : IDomainEventHandler<PaymentCapturedDomainEvent>
{
    private readonly IOrderRepository _orders;
    private readonly ICustomerRepository _customers;
    private readonly IEmailService _emailService;

    public PaymentCapturedDomainEventHandler(IOrderRepository orders, ICustomerRepository customers, IEmailService emailService)
    {
        _orders = orders;
        _customers = customers;
        _emailService = emailService;
    }

    public async Task HandleAsync(PaymentCapturedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var order = await _orders.GetByIdAsync(domainEvent.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), domainEvent.OrderId);
        var customer = await _customers.GetByIdAsync(order.CustomerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), order.CustomerId);

        await _emailService.SendAsync(
            customer.Email.ToString(),
            "Payment captured",
            $"Your payment for order {domainEvent.OrderId} has been captured. Amount: {domainEvent.Amount}.",
            cancellationToken);
    }
}

using EnterpriseOrderManagementSystem.Application.Abstractions.Services;
using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Application.Common.Interfaces;
using EnterpriseOrderManagementSystem.Domain.Aggregates.CustomerAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.CustomerAggregate.Events;
using EnterpriseOrderManagementSystem.Domain.Repositories;

namespace EnterpriseOrderManagementSystem.Application.EventHandlers.Customers;

public sealed class CustomerDeactivatedDomainEventHandler : IDomainEventHandler<CustomerDeactivatedDomainEvent>
{
    private readonly ICustomerRepository _customers; 
    private readonly IEmailService _emailService;
    
    public CustomerDeactivatedDomainEventHandler(ICustomerRepository customers, 
                                                    IEmailService emailService) 
    { 
        _customers = customers; 
        _emailService = emailService; 
    }
    
    public async Task HandleAsync(CustomerDeactivatedDomainEvent domainEvent, 
                                    CancellationToken cancellationToken = default)
    {
        var customer = await _customers.GetByIdAsync(domainEvent.CustomerId, cancellationToken) ??
                            throw new NotFoundException(nameof(Customer), domainEvent.CustomerId);
       
        await _emailService.SendAsync(customer.Email.ToString(), 
                                        "Customer account deactivated", 
                                            $"Hello {customer.FirstName}, your customer account has been deactivated.", 
                                                cancellationToken);
    }
}

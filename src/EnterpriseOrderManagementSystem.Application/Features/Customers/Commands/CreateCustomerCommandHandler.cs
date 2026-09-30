using EnterpriseOrderManagementSystem.Domain.Aggregates.CustomerAggregate;
using EnterpriseOrderManagementSystem.Domain.Common.ValueObjects;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Customers.Commands;

public sealed class CreateCustomerCommandHandler
    : IRequestHandler<CreateCustomerCommand, Guid>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCustomerCommandHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(
        CreateCustomerCommand request,
        CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);

        var phoneNumber = PhoneNumber.Create(
            request.PhoneCountryCode,
            request.PhoneNationalNumber);

        var address = Address.Create(
            request.Street,
            request.City,
            request.State,
            request.Country,
            request.PostalCode);

        var customer = Customer.Create(
            request.FirstName,
            request.LastName,
            email,
            phoneNumber,
            address);

        await _customerRepository.AddAsync(
            customer,
            cancellationToken);

        return customer.Id;
    }
}
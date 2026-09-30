using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Customers.Commands;

public sealed class DeactivateCustomerCommandHandler
    : IRequestHandler<DeactivateCustomerCommand, Guid>
{
    #region Fields

    private readonly ICustomerRepository _customerRepository;

    #endregion

    #region Constructor

    public DeactivateCustomerCommandHandler(
        ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    #endregion

    #region Handle

    public async Task<Guid> Handle(
        DeactivateCustomerCommand request,
        CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(
            request.CustomerId,
            cancellationToken);

        if (customer is null)
        {
            throw new NotFoundException(
                "Customer",
                request.CustomerId);
        }

        customer.Deactivate();

        return customer.Id;
    }

    #endregion
}
using EnterpriseOrderManagementSystem.Application.DTOs.Customers;
using EnterpriseOrderManagementSystem.Application.Mapping;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Customers.Queries;

public sealed class GetCustomerByIdQueryHandler
    : IRequestHandler<GetCustomerByIdQuery, CustomerDto?>
{
    #region Fields

    private readonly ICustomerRepository _customerRepository;

    #endregion

    #region Constructor

    public GetCustomerByIdQueryHandler(
        ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    #endregion

    #region Handle

    public async Task<CustomerDto?> Handle(
        GetCustomerByIdQuery request,
        CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(
            request.CustomerId,
            cancellationToken);

        return customer?.ToDto();
    }

    #endregion

}
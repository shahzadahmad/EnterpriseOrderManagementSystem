using EnterpriseOrderManagementSystem.Application.DTOs.Customers;
using EnterpriseOrderManagementSystem.Domain.Aggregates.CustomerAggregate;

namespace EnterpriseOrderManagementSystem.Application.Mapping;

public static class CustomerMapping
{
    public static CustomerDto ToDto(this Customer customer)
    {
        ArgumentNullException.ThrowIfNull(customer);

        return new CustomerDto(
            customer.Id,
            customer.FirstName,
            customer.LastName,
            customer.Email.Value,
            customer.PhoneNumber.CountryCode,
            customer.PhoneNumber.NationalNumber,
            customer.Address.Street,
            customer.Address.City,
            customer.Address.State,
            customer.Address.Country,
            customer.Address.PostalCode,
            customer.Status,
            customer.RegisteredOnUtc);
    }
}
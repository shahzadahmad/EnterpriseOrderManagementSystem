using EnterpriseOrderManagementSystem.Application.Common.Interfaces;

namespace EnterpriseOrderManagementSystem.Application.Features.Customers.Commands;

public sealed record CreateCustomerCommand(
    string FirstName,
    string LastName,
    string Email,
    string PhoneCountryCode,
    string PhoneNationalNumber,
    string Street,
    string City,
    string State,
    string Country,
    string PostalCode) : ICommand<Guid>;
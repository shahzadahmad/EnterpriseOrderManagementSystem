using EnterpriseOrderManagementSystem.Domain.Enums;

namespace EnterpriseOrderManagementSystem.Application.DTOs.Customers;

public sealed record CustomerDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneCountryCode,
    string PhoneNationalNumber,
    string Street,
    string City,
    string State,
    string Country,
    string PostalCode,
    CustomerStatus Status,
    DateTime RegisteredOnUtc);
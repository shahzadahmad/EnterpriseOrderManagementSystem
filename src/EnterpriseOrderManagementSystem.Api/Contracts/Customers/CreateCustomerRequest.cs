
using System.ComponentModel.DataAnnotations;

namespace EnterpriseOrderManagementSystem.API.Contracts.Customers;

#region Create Customer Request

public sealed record CreateCustomerRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string FirstName { get; init; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string LastName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(320)]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string PhoneCountryCode { get; init; } = string.Empty;

    [Required]
    public string PhoneNationalNumber { get; init; } = string.Empty;

    [Required]
    public string Street { get; init; } = string.Empty;

    [Required]
    public string City { get; init; } = string.Empty;

    [Required]
    public string State { get; init; } = string.Empty;

    [Required]
    public string Country { get; init; } = string.Empty;

    [Required]
    public string PostalCode { get; init; } = string.Empty;
}

#endregion

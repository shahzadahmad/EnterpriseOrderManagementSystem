using EnterpriseOrderManagementSystem.Domain.Common.Base;
using EnterpriseOrderManagementSystem.Domain.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Common.ValueObjects;
using EnterpriseOrderManagementSystem.Domain.Enums;
using EnterpriseOrderManagementSystem.Domain.Aggregates.CustomerAggregate.Events;

namespace EnterpriseOrderManagementSystem.Domain.Aggregates.CustomerAggregate;

public sealed class Customer : AggregateRoot<Guid>
{
    #region Constructors

    private Customer()
    {
    }

    private Customer(
        string firstName,
        string lastName,
        Email email,
        PhoneNumber phoneNumber,
        Address address)
    {
        Id = Guid.NewGuid();

        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        Address = address;

        Status = CustomerStatus.Active;
        RegisteredOnUtc = DateTime.UtcNow;
    }

    #endregion

    #region Properties

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public Email Email { get; private set; } = default!;

    public PhoneNumber PhoneNumber { get; private set; } = default!;

    public Address Address { get; private set; } = default!;

    public CustomerStatus Status { get; private set; }

    public DateTime RegisteredOnUtc { get; private set; }

    #endregion

    #region Factory Methods

    public static Customer Create(
        string firstName,
        string lastName,
        Email email,
        PhoneNumber phoneNumber,
        Address address)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);

        var customer = new Customer(
            firstName.Trim(),
            lastName.Trim(),
            email,
            phoneNumber,
            address);

        customer.AddDomainEvent(
            new CustomerRegisteredDomainEvent(customer.Id));

        return customer;
    }

    #endregion

    #region Update Methods

    public void UpdateName(string firstName, string lastName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
    }

    public void ChangeEmail(Email email)
    {
        Email = email;
    }

    public void ChangePhoneNumber(PhoneNumber phoneNumber)
    {
        PhoneNumber = phoneNumber;
    }

    public void ChangeAddress(Address address)
    {
        Address = address;
    }

    #endregion

    #region Status Management

    public void Activate()
    {
        if (Status == CustomerStatus.Active)
            throw new BusinessRuleViolationException("Customer is already active.");

        Status = CustomerStatus.Active;

        AddDomainEvent(new CustomerActivatedDomainEvent(Id));
    }

    public void Deactivate()
    {
        if (Status == CustomerStatus.Inactive)
            throw new BusinessRuleViolationException("Customer is already inactive.");

        Status = CustomerStatus.Inactive;

        AddDomainEvent(new CustomerDeactivatedDomainEvent(Id));
    }

    #endregion
}
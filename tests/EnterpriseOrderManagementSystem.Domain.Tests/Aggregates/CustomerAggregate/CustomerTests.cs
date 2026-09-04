using EnterpriseOrderManagementSystem.Domain.Aggregates.CustomerAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.CustomerAggregate.Events;
using EnterpriseOrderManagementSystem.Domain.Common.ValueObjects;
using FluentAssertions;
using System.Net;

namespace EnterpriseOrderManagementSystem.Domain.Tests.Aggregates.CustomerAggregate;

public sealed class CustomerTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidData_CreatesCustomer()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var email = Email.Create("john.doe@example.com");
        var phoneNumber = PhoneNumber.Create("+92", "3001234567");
        var address = Address.Create(
                street: "123 Main Street",
                city: "Islamabad",
                state: "Punjab",
                postalCode: "44000",
                country: "Pakistan");

        // Act
        var customer = Customer.Create(
            firstName,
            lastName,
            email,
            phoneNumber,
            address);

        // Assert
        customer.Should().NotBeNull();

        customer.Id
            .Should()
            .NotBe(Guid.Empty);

        customer.FirstName
            .Should()
            .Be(firstName);

        customer.LastName
            .Should()
            .Be(lastName);

        customer.Email
            .Should()
            .Be(email);

        customer.PhoneNumber
            .Should()
            .Be(phoneNumber);

        customer.Address
            .Should()
            .Be(address);
    }

    [Fact]
    public void Create_WithValidData_GeneratesCustomerId()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var email = Email.Create("john.doe@example.com");
        var phoneNumber = PhoneNumber.Create("+92", "3001234567");
        var address = Address.Create(
                street: "123 Main Street",
                city: "Islamabad",
                state: "Punjab",
                postalCode: "44000",
                country: "Pakistan");

        // Act
        var customer = Customer.Create(
            firstName,
            lastName,
            email,
            phoneNumber,
            address);

        // Assert
        customer.Id
            .Should()
            .NotBe(Guid.Empty);
    }

    [Fact]
    public void Create_WithValidData_GeneratesUniqueCustomerIds()
    {
        // Arrange
        var firstEmail = Email.Create("john.doe@example.com");
        var secondEmail = Email.Create("jane.doe@example.com");

        var firstPhone = PhoneNumber.Create("+92", "3001234567");
        var secondPhone = PhoneNumber.Create("+92", "3001234568");

        var firstAddress = CreateValidAddress();
        var secondAddress = CreateValidAddress();

        // Act
        var customer1 = Customer.Create(
            "John",
            "Doe",
            firstEmail,
            firstPhone,
            firstAddress);

        var customer2 = Customer.Create(
            "Jane",
            "Doe",
            secondEmail,
            secondPhone,
            secondAddress);

        // Assert
        customer1.Id
            .Should()
            .NotBe(Guid.Empty);

        customer2.Id
            .Should()
            .NotBe(Guid.Empty);

        customer1.Id
            .Should()
            .NotBe(customer2.Id);
    }

    #endregion

    #region First Name Validation Tests

    [Fact]
    public void Create_WithEmptyFirstName_ThrowsArgumentException()
    {
        // Arrange
        var email = CreateValidEmail();
        var phoneNumber = CreateValidPhoneNumber();
        var address = CreateValidAddress();

        // Act
        var action = () =>
            Customer.Create(
                string.Empty,
                "Doe",
                email,
                phoneNumber,
                address);

        // Assert
        action.Should()
            .Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithWhitespaceFirstName_ThrowsArgumentException()
    {
        // Arrange
        var email = CreateValidEmail();
        var phoneNumber = CreateValidPhoneNumber();
        var address = CreateValidAddress();

        // Act
        var action = () =>
            Customer.Create(
                "   ",
                "Doe",
                email,
                phoneNumber,
                address);

        // Assert
        action.Should()
            .Throw<ArgumentException>();
    }

    #endregion

    #region Last Name Validation Tests

    [Fact]
    public void Create_WithEmptyLastName_ThrowsArgumentException()
    {
        // Arrange
        var email = CreateValidEmail();
        var phoneNumber = CreateValidPhoneNumber();
        var address = CreateValidAddress();

        // Act
        var action = () =>
            Customer.Create(
                "John",
                string.Empty,
                email,
                phoneNumber,
                address);

        // Assert
        action.Should()
            .Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithWhitespaceLastName_ThrowsArgumentException()
    {
        // Arrange
        var email = CreateValidEmail();
        var phoneNumber = CreateValidPhoneNumber();
        var address = CreateValidAddress();

        // Act
        var action = () =>
            Customer.Create(
                "John",
                "   ",
                email,
                phoneNumber,
                address);

        // Assert
        action.Should()
            .Throw<ArgumentException>();
    }

    #endregion

    #region Trimming Tests

    [Fact]
    public void Create_WithNamesContainingWhitespace_TrimsNames()
    {
        // Arrange
        var email = CreateValidEmail();
        var phoneNumber = CreateValidPhoneNumber();
        var address = CreateValidAddress();

        // Act
        var customer = Customer.Create(
            "  John  ",
            "  Doe  ",
            email,
            phoneNumber,
            address);

        // Assert
        customer.FirstName
            .Should()
            .Be("John");

        customer.LastName
            .Should()
            .Be("Doe");
    }

    #endregion

    #region Value Object Tests

    [Fact]
    public void Create_WithValidEmail_AssignsEmail()
    {
        // Arrange
        var email = Email.Create("john.doe@example.com");
        var phoneNumber = CreateValidPhoneNumber();
        var address = CreateValidAddress();

        // Act
        var customer = Customer.Create(
            "John",
            "Doe",
            email,
            phoneNumber,
            address);

        // Assert
        customer.Email
            .Should()
            .Be(email);
    }

    [Fact]
    public void Create_WithValidPhoneNumber_AssignsPhoneNumber()
    {
        // Arrange
        var email = CreateValidEmail();
        var phoneNumber = CreateValidPhoneNumber();
        var address = CreateValidAddress();

        // Act
        var customer = Customer.Create(
            "John",
            "Doe",
            email,
            phoneNumber,
            address);

        // Assert
        customer.PhoneNumber
            .Should()
            .Be(phoneNumber);
    }

    [Fact]
    public void Create_WithValidAddress_AssignsAddress()
    {
        // Arrange
        var email = CreateValidEmail();
        var phoneNumber = CreateValidPhoneNumber();
        var address = CreateValidAddress();

        // Act
        var customer = Customer.Create(
            "John",
            "Doe",
            email,
            phoneNumber,
            address);

        // Assert
        customer.Address
            .Should()
            .Be(address);
    }

    #endregion

    #region Domain Event Tests

    [Fact]
    public void Create_WithValidData_RaisesCustomerRegisteredDomainEvent()
    {
        // Arrange
        var customer = CreateValidCustomer();

        // Assert
        customer.DomainEvents
            .Should()
            .ContainSingle(eventItem =>
                eventItem is CustomerRegisteredDomainEvent);
    }

    [Fact]
    public void Create_WithValidData_CustomerRegisteredEventContainsCustomerId()
    {
        // Arrange
        var customer = CreateValidCustomer();

        // Act
        var domainEvent = customer.DomainEvents
            .OfType<CustomerRegisteredDomainEvent>()
            .Single();

        // Assert
        domainEvent.CustomerId
            .Should()
            .Be(customer.Id);
    }

    #endregion

    #region Test Helpers

    private static Customer CreateValidCustomer()
    {
        return Customer.Create(
            "John",
            "Doe",
            CreateValidEmail(),
            CreateValidPhoneNumber(),
            CreateValidAddress());
    }

    private static Email CreateValidEmail()
    {
        return Email.Create(
            "john.doe@example.com");
    }

    private static PhoneNumber CreateValidPhoneNumber()
    {
        return PhoneNumber.Create(
            "+92", "3001234567");
    }

    private static Address CreateValidAddress()
    {
        return Address.Create(        
            street: "123 Main Street",
            city: "Islamabad",
            state: "Punjab",
            postalCode: "44000",
            country: "Pakistan");
    }

    #endregion
}
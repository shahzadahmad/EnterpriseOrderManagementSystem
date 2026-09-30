using EnterpriseOrderManagementSystem.Application.Features.Customers.Commands;
using FluentValidation;

namespace EnterpriseOrderManagementSystem.Application.Validators.Customers;

public sealed class DeactivateCustomerCommandValidator
    : AbstractValidator<DeactivateCustomerCommand>
{
    public DeactivateCustomerCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty();
    }
}
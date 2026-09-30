using EnterpriseOrderManagementSystem.Application.Features.Customers.Commands;
using FluentValidation;

namespace EnterpriseOrderManagementSystem.Application.Validators.Customers;

public sealed class ActivateCustomerCommandValidator
    : AbstractValidator<ActivateCustomerCommand>
{
    public ActivateCustomerCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty();
    }
}
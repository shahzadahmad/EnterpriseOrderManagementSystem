using EnterpriseOrderManagementSystem.Application.Features.Payments.Commands;
using FluentValidation;

namespace EnterpriseOrderManagementSystem.Application.Validators.Payments;

public sealed class AuthorizePaymentCommandValidator
    : AbstractValidator<AuthorizePaymentCommand>
{
    public AuthorizePaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty();
    }
}
using EnterpriseOrderManagementSystem.Application.Features.Payments.Commands;
using FluentValidation;

namespace EnterpriseOrderManagementSystem.Application.Validators.Payments;

public sealed class FailPaymentCommandValidator
    : AbstractValidator<FailPaymentCommand>
{
    public FailPaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty();

        RuleFor(x => x.FailureCode)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.FailureReason)
            .NotEmpty()
            .MaximumLength(500);
    }
}
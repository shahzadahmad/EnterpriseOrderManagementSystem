using EnterpriseOrderManagementSystem.Application.Features.Payments.Commands;
using FluentValidation;

namespace EnterpriseOrderManagementSystem.Application.Validators.Payments;

public sealed class CancelPaymentCommandValidator
    : AbstractValidator<CancelPaymentCommand>
{
    public CancelPaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty();

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(500);
    }
}
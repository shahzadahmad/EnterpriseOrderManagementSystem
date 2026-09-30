using EnterpriseOrderManagementSystem.Application.Features.Payments.Commands;
using FluentValidation;

namespace EnterpriseOrderManagementSystem.Application.Validators.Payments;

public sealed class SettlePaymentCommandValidator
    : AbstractValidator<SettlePaymentCommand>
{
    public SettlePaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty();
    }
}
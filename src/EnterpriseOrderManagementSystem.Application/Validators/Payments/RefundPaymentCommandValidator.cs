using EnterpriseOrderManagementSystem.Application.Features.Payments.Commands;
using FluentValidation;

namespace EnterpriseOrderManagementSystem.Application.Validators.Payments;

public sealed class RefundPaymentCommandValidator
    : AbstractValidator<RefundPaymentCommand>
{
    public RefundPaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty();

        RuleFor(x => x.RefundAmount)
            .GreaterThan(0)
            .Must(HaveMaximumTwoDecimalPlaces)
            .WithMessage("Refund amount cannot contain more than two decimal places.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(500);
    }

    private static bool HaveMaximumTwoDecimalPlaces(decimal amount)
    {
        return decimal.Round(amount, 2) == amount;
    }
}
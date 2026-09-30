using EnterpriseOrderManagementSystem.Application.Features.Payments.Commands;
using FluentValidation;

namespace EnterpriseOrderManagementSystem.Application.Validators.Payments;

public sealed class CreatePaymentCommandValidator
    : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty();

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .Must(HaveMaximumTwoDecimalPlaces)
            .WithMessage("Amount cannot contain more than two decimal places.");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3);

        RuleFor(x => x.PaymentMethod)
            .IsInEnum();

        RuleFor(x => x.PaymentProvider)
            .IsInEnum();
    }

    private static bool HaveMaximumTwoDecimalPlaces(decimal amount)
    {
        return decimal.Round(amount, 2) == amount;
    }
}
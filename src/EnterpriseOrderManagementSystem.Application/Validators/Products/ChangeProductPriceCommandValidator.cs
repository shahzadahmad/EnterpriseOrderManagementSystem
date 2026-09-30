using EnterpriseOrderManagementSystem.Application.Features.Products.Commands;
using FluentValidation;

namespace EnterpriseOrderManagementSystem.Application.Validators.Products;

public sealed class ChangeProductPriceCommandValidator
    : AbstractValidator<ChangeProductPriceCommand>
{
    public ChangeProductPriceCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty();

        RuleFor(x => x.Price)
            .GreaterThan(0);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .MaximumLength(10);
    }
}
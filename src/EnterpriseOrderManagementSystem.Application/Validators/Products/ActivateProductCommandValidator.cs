using EnterpriseOrderManagementSystem.Application.Features.Products.Commands;
using FluentValidation;

namespace EnterpriseOrderManagementSystem.Application.Validators.Products;

public sealed class ActivateProductCommandValidator
    : AbstractValidator<ActivateProductCommand>
{
    public ActivateProductCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty();
    }
}
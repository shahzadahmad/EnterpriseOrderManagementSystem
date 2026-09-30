using EnterpriseOrderManagementSystem.Application.Features.Products.Commands;
using FluentValidation;

namespace EnterpriseOrderManagementSystem.Application.Validators.Products;

public sealed class DeactivateProductCommandValidator
    : AbstractValidator<DeactivateProductCommand>
{
    public DeactivateProductCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty();
    }
}
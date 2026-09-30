using EnterpriseOrderManagementSystem.Application.Features.Products.Commands;
using FluentValidation;

namespace EnterpriseOrderManagementSystem.Application.Validators.Products;

public sealed class DiscontinueProductCommandValidator
    : AbstractValidator<DiscontinueProductCommand>
{
    public DiscontinueProductCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty();
    }
}

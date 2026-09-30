using EnterpriseOrderManagementSystem.Application.Common.Models;
using FluentValidation;

namespace EnterpriseOrderManagementSystem.Application.Validators;

public sealed class PaginationRequestValidator
    : AbstractValidator<PaginationRequest>
{
    public PaginationRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}
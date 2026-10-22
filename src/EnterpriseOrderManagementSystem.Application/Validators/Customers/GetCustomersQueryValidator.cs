using EnterpriseOrderManagementSystem.Application.Features.Customers.Queries;
using FluentValidation;

namespace EnterpriseOrderManagementSystem.Application.Validators.Customers;

public sealed class GetCustomersQueryValidator
    : AbstractValidator<GetCustomersQuery>
{
    public GetCustomersQueryValidator()
    {
        RuleFor(x => x.Pagination)
            .NotNull()
            .SetValidator(new PaginationRequestValidator());
    }
}
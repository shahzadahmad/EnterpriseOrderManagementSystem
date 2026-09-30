
// ============================================================
// AddOrderItemCommandValidator.cs
// ============================================================

using EnterpriseOrderManagementSystem.Application.Features.Orders.Commands;
using FluentValidation;

namespace EnterpriseOrderManagementSystem.Application.Validators.Orders;

public sealed class AddOrderItemCommandValidator
    : AbstractValidator<AddOrderItemCommand>
{
    public AddOrderItemCommandValidator()
    {
        #region Order Validation

        RuleFor(x => x.OrderId)
            .NotEmpty();

        #endregion

        #region Product Validation

        RuleFor(x => x.ProductId)
            .NotEmpty();

        #endregion

        #region Quantity Validation

        RuleFor(x => x.Quantity)
            .GreaterThan(0);

        #endregion
    }
}

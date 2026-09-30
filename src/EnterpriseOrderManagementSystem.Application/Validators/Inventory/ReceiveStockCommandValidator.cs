// ============================================================
// ReceiveStockCommandValidator.cs
// ============================================================

using EnterpriseOrderManagementSystem.Application.Features.Inventory.Commands;
using FluentValidation;

namespace EnterpriseOrderManagementSystem.Application.Validators.Inventory;

public sealed class ReceiveStockCommandValidator
    : AbstractValidator<ReceiveStockCommand>
{
    public ReceiveStockCommandValidator()
    {
        #region Inventory Validation

        RuleFor(x => x.InventoryId)
            .NotEmpty();

        #endregion

        #region Quantity Validation

        RuleFor(x => x.Quantity)
            .GreaterThan(0);

        #endregion

        #region Reference Validation

        RuleFor(x => x.Reference)
            .NotEmpty()
            .MaximumLength(200);

        #endregion

        #region Reason Validation

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(500);

        #endregion
    }
}

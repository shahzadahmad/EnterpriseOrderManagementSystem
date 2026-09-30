// ============================================================
// DiscontinueInventoryCommandValidator.cs
// ============================================================

using EnterpriseOrderManagementSystem.Application.Features.Inventory.Commands;
using FluentValidation;

namespace EnterpriseOrderManagementSystem.Application.Validators.Inventory;

public sealed class DiscontinueInventoryCommandValidator
    : AbstractValidator<DiscontinueInventoryCommand>
{
    public DiscontinueInventoryCommandValidator()
    {
        #region Inventory Validation

        RuleFor(x => x.InventoryId)
            .NotEmpty();

        #endregion

        #region Reason Validation

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(500);

        #endregion
    }
}
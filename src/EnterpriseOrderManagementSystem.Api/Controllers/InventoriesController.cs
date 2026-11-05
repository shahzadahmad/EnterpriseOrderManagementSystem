
using EnterpriseOrderManagementSystem.API.Contracts.Inventory;
using EnterpriseOrderManagementSystem.Application.Common.Models;
using EnterpriseOrderManagementSystem.Application.DTOs.Inventory;
using EnterpriseOrderManagementSystem.Application.Features.Inventory.Commands;
using EnterpriseOrderManagementSystem.Application.Features.Inventory.Queries;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseOrderManagementSystem.API.Controllers;

[Route("api/[controller]")]
public sealed class InventoriesController : BaseApiController
{
    public InventoriesController(MediatR.ISender sender)
        : base(sender)
    {
    }

    /// <summary>Creates inventory for a product in a warehouse.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateInventoryRequest request,
        CancellationToken cancellationToken)
    {
        var inventoryId = await Sender.Send(
            new CreateInventoryCommand(
                request.ProductId,
                request.WarehouseId,
                request.ReorderLevel,
                request.MaximumQuantity),
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = inventoryId },
            new { id = inventoryId });
    }

    /// <summary>Retrieves inventory, including its transactions.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(InventoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InventoryDto>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var inventory = await Sender.Send(
            new GetInventoryByIdQuery(id),
            cancellationToken);

        if (inventory is null)
        {
            return NotFound();
        }

        return Ok(inventory);
    }

    /// <summary>Retrieves inventory using pagination.</summary>
    [HttpGet]
    [ProducesResponseType(
        typeof(PaginatedResult<InventoryDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedResult<InventoryDto>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new
            {
                message =
                    "pageNumber must be at least 1, and pageSize must be between 1 and 100."
            });
        }

        var result = await Sender.Send(
            new GetInventoriesQuery(
                new PaginationRequest(pageNumber, pageSize)),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>Receives stock into inventory.</summary>
    [HttpPost("{id:guid}/receive")]
    public async Task<IActionResult> ReceiveStock(
        [FromRoute] Guid id,
        [FromBody] StockMovementRequest request,
        CancellationToken cancellationToken)
    {
        var inventoryId = await Sender.Send(
            new ReceiveStockCommand(
                id,
                request.Quantity,
                request.Reference,
                request.Reason),
            cancellationToken);

        return Ok(new
        {
            id = inventoryId,
            message = "Stock received successfully."
        });
    }

    /// <summary>Reserves available stock.</summary>
    [HttpPost("{id:guid}/reserve")]
    public async Task<IActionResult> ReserveStock(
        [FromRoute] Guid id,
        [FromBody] StockMovementRequest request,
        CancellationToken cancellationToken)
    {
        var inventoryId = await Sender.Send(
            new ReserveStockCommand(
                id,
                request.Quantity,
                request.Reference,
                request.Reason),
            cancellationToken);

        return Ok(new
        {
            id = inventoryId,
            message = "Stock reserved successfully."
        });
    }

    /// <summary>Releases previously reserved stock.</summary>
    [HttpPost("{id:guid}/release")]
    public async Task<IActionResult> ReleaseReservedStock(
        [FromRoute] Guid id,
        [FromBody] StockMovementRequest request,
        CancellationToken cancellationToken)
    {
        var inventoryId = await Sender.Send(
            new ReleaseReservedStockCommand(
                id,
                request.Quantity,
                request.Reference,
                request.Reason),
            cancellationToken);

        return Ok(new
        {
            id = inventoryId,
            message = "Reserved stock released successfully."
        });
    }

    /// <summary>Commits a stock reservation.</summary>
    [HttpPost("{id:guid}/commit")]
    public async Task<IActionResult> CommitReservation(
        [FromRoute] Guid id,
        [FromBody] StockMovementRequest request,
        CancellationToken cancellationToken)
    {
        var inventoryId = await Sender.Send(
            new CommitReservationCommand(
                id,
                request.Quantity,
                request.Reference,
                request.Reason),
            cancellationToken);

        return Ok(new
        {
            id = inventoryId,
            message = "Stock reservation committed successfully."
        });
    }

    /// <summary>Adjusts the available stock quantity.</summary>
    [HttpPatch("{id:guid}/adjust")]
    public async Task<IActionResult> AdjustStock(
        [FromRoute] Guid id,
        [FromBody] AdjustStockRequest request,
        CancellationToken cancellationToken)
    {
        var inventoryId = await Sender.Send(
            new AdjustStockCommand(
                id,
                request.NewAvailableQuantity,
                request.Reference,
                request.Reason),
            cancellationToken);

        return Ok(new
        {
            id = inventoryId,
            message = "Inventory stock adjusted successfully."
        });
    }

    /// <summary>Discontinues an inventory record.</summary>
    [HttpPatch("{id:guid}/discontinue")]
    public async Task<IActionResult> Discontinue(
        [FromRoute] Guid id,
        [FromBody] DiscontinueInventoryRequest request,
        CancellationToken cancellationToken)
    {
        var inventoryId = await Sender.Send(
            new DiscontinueInventoryCommand(id, request.Reason),
            cancellationToken);

        return Ok(new
        {
            id = inventoryId,
            status = "Discontinued"
        });
    }

    /// <summary>Reactivates a discontinued inventory record.</summary>
    [HttpPatch("{id:guid}/reactivate")]
    public async Task<IActionResult> Reactivate(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var inventoryId = await Sender.Send(
            new ReactivateInventoryCommand(id),
            cancellationToken);

        return Ok(new
        {
            id = inventoryId,
            message = "Inventory reactivated successfully."
        });
    }
}

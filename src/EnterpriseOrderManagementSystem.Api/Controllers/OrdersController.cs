
using EnterpriseOrderManagementSystem.API.Contracts.Orders;
using EnterpriseOrderManagementSystem.Application.Common.Models;
using EnterpriseOrderManagementSystem.Application.DTOs.Orders;
using EnterpriseOrderManagementSystem.Application.Features.Orders.Commands;
using EnterpriseOrderManagementSystem.Application.Features.Orders.Queries;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseOrderManagementSystem.API.Controllers;

[Route("api/[controller]")]
public sealed class OrdersController : BaseApiController
{
    public OrdersController(MediatR.ISender sender) : base(sender)
    {
    }

    #region Create Order

    /// <summary>Creates a pending order for an existing customer.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var orderId = await Sender.Send(
            new CreateOrderCommand(request.CustomerId),
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = orderId },
            new { id = orderId });
    }

    #endregion

    #region Get Order By Id

    /// <summary>Retrieves an order, including its items.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var order = await Sender.Send(
            new GetOrderByIdQuery(id),
            cancellationToken);

        if (order is null)
        {
            return NotFound();
        }

        return Ok(order);
    }

    #endregion

    #region Get Orders

    /// <summary>Retrieves orders using pagination.</summary>
    [HttpGet]
    [ProducesResponseType(
        typeof(PaginatedResult<OrderDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedResult<OrderDto>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new
            {
                message = "pageNumber must be at least 1, and pageSize must be between 1 and 100."
            });
        }

        var result = await Sender.Send(
            new GetOrdersQuery(
                new PaginationRequest(pageNumber, pageSize)),
            cancellationToken);

        return Ok(result);
    }

    #endregion

    #region Add Order Item

    /// <summary>Adds a product to a pending order.</summary>
    [HttpPost("{id:guid}/items")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddItem(
        [FromRoute] Guid id,
        [FromBody] AddOrderItemRequest request,
        CancellationToken cancellationToken)
    {
        var orderId = await Sender.Send(
            new AddOrderItemCommand(
                id,
                request.ProductId,
                request.Quantity),
            cancellationToken);

        return Ok(new
        {
            id = orderId,
            message = "Order item added successfully."
        });
    }

    #endregion

    #region Confirm Order

    /// <summary>Confirms a pending order.</summary>
    [HttpPatch("{id:guid}/confirm")]
    public async Task<IActionResult> Confirm(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var orderId = await Sender.Send(
            new ConfirmOrderCommand(id),
            cancellationToken);

        return Ok(new { id = orderId, status = "Confirmed" });
    }

    #endregion

    #region Cancel Order

    /// <summary>Cancels an order with a reason.</summary>
    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(
        [FromRoute] Guid id,
        [FromBody] CancelOrderRequest request,
        CancellationToken cancellationToken)
    {
        var orderId = await Sender.Send(
            new CancelOrderCommand(id, request.Reason),
            cancellationToken);

        return Ok(new { id = orderId, status = "Cancelled" });
    }

    #endregion

    #region Ship Order

    /// <summary>Marks a confirmed order as shipped.</summary>
    [HttpPatch("{id:guid}/ship")]
    public async Task<IActionResult> Ship(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var orderId = await Sender.Send(
            new ShipOrderCommand(id),
            cancellationToken);

        return Ok(new { id = orderId, status = "Shipped" });
    }

    #endregion

    #region Deliver Order

    /// <summary>Marks a shipped order as delivered.</summary>
    [HttpPatch("{id:guid}/deliver")]
    public async Task<IActionResult> Deliver(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var orderId = await Sender.Send(
            new DeliverOrderCommand(id),
            cancellationToken);

        return Ok(new { id = orderId, status = "Delivered" });
    }

    #endregion
}
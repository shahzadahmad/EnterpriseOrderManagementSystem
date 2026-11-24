using EnterpriseOrderManagementSystem.API.Contracts.Payments;
using EnterpriseOrderManagementSystem.Application.Common.Models;
using EnterpriseOrderManagementSystem.Application.DTOs.Payments;
using EnterpriseOrderManagementSystem.Application.Features.Payments.Commands;
using EnterpriseOrderManagementSystem.Application.Features.Payments.Queries;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseOrderManagementSystem.API.Controllers;

[Route("api/[controller]")]
public sealed class PaymentsController : BaseApiController
{
    public PaymentsController(MediatR.ISender sender)
        : base(sender)
    {
    }

    #region Create Payment

    /// <summary>
    /// Creates a payment for an existing order.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        var paymentId = await Sender.Send(
            new CreatePaymentCommand(
                request.OrderId,
                request.Amount,
                request.Currency,
                request.PaymentMethod,
                request.PaymentProvider),
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = paymentId },
            new { id = paymentId });
    }

    #endregion

    #region Get Payment By Id

    /// <summary>
    /// Retrieves a payment including its transactions.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(
        typeof(PaymentDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentDto>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var payment = await Sender.Send(
            new GetPaymentByIdQuery(id),
            cancellationToken);

        if (payment is null)
        {
            return NotFound();
        }

        return Ok(payment);
    }

    #endregion

    #region Get Payments

    /// <summary>
    /// Retrieves payments using pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(
        typeof(PaginatedResult<PaymentDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedResult<PaymentDto>>> GetAll(
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
            new GetPaymentsQuery(
                new PaginationRequest(pageNumber, pageSize)),
            cancellationToken);

        return Ok(result);
    }

    #endregion

    #region Authorize Payment

    /// <summary>
    /// Authorizes a payment through the configured payment gateway.
    /// </summary>
    [HttpPost("{id:guid}/authorize")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Authorize(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var paymentId = await Sender.Send(
            new AuthorizePaymentCommand(id),
            cancellationToken);

        return Ok(new
        {
            id = paymentId,
            message = "Payment authorization processed successfully."
        });
    }

    #endregion

    #region Capture Payment

    /// <summary>
    /// Captures an authorized payment through the payment gateway.
    /// </summary>
    [HttpPost("{id:guid}/capture")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Capture(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var paymentId = await Sender.Send(
            new CapturePaymentCommand(id),
            cancellationToken);

        return Ok(new
        {
            id = paymentId,
            message = "Payment capture processed successfully."
        });
    }

    #endregion

    #region Refund Payment

    /// <summary>
    /// Refunds all or part of a captured payment.
    /// </summary>
    [HttpPost("{id:guid}/refund")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Refund(
        [FromRoute] Guid id,
        [FromBody] RefundPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var paymentId = await Sender.Send(
            new RefundPaymentCommand(
                id,
                request.RefundAmount,
                request.Reason),
            cancellationToken);

        return Ok(new
        {
            id = paymentId,
            message = "Payment refund processed successfully."
        });
    }

    #endregion

    #region Cancel Payment

    /// <summary>
    /// Cancels a payment with a reason.
    /// </summary>
    [HttpPatch("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(
        [FromRoute] Guid id,
        [FromBody] CancelPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var paymentId = await Sender.Send(
            new CancelPaymentCommand(id, request.Reason),
            cancellationToken);

        return Ok(new
        {
            id = paymentId,
            status = "Cancelled"
        });
    }

    #endregion
}
using EnterpriseOrderManagementSystem.API.Contracts.Customers;
using EnterpriseOrderManagementSystem.Application.Common.Models;
using EnterpriseOrderManagementSystem.Application.DTOs.Customers;
using EnterpriseOrderManagementSystem.Application.Features.Customers.Commands;
using EnterpriseOrderManagementSystem.Application.Features.Customers.Queries;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseOrderManagementSystem.API.Controllers;

#region Customers Controller

[Route("api/[controller]")]
public sealed class CustomersController : BaseApiController
{
    #region Constructor

    public CustomersController(MediatR.ISender sender) : base(sender) { }

    #endregion

    #region Create Customer

    /// <summary>Creates a new customer.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateCustomerCommand(
            request.FirstName, request.LastName, request.Email,
            request.PhoneCountryCode, request.PhoneNationalNumber,
            request.Street, request.City, request.State,
            request.Country, request.PostalCode);

        var customerId = await Sender.Send(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetById), new { id = customerId }, new { id = customerId });
    }

    #endregion

    #region Get Customer By Id

    /// <summary>Retrieves a customer by its identifier.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerDto>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var customer = await Sender.Send(
            new GetCustomerByIdQuery(id), 
            cancellationToken);

        if (customer is null) 
            return NotFound();

        return Ok(customer);
    }

    #endregion

    #region Get Customers

    /// <summary>Retrieves customers using page number and page size.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedResult<CustomerDto>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var pagination = new PaginationRequest(
               pageNumber,
               pageSize);

        var customers = await Sender.Send(
            new GetCustomersQuery(pagination),
            cancellationToken);

        return Ok(customers);
    }

    #endregion

    #region Activate Customer

    /// <summary>Activates an existing customer.</summary>
    [HttpPatch("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Activate(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var customerId = await Sender.Send(new ActivateCustomerCommand(id), cancellationToken);
        return Ok(new { id = customerId, status = "Active" });
    }

    #endregion

    #region Deactivate Customer

    /// <summary>Deactivates an existing customer.</summary>
    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Deactivate(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var customerId = await Sender.Send(new DeactivateCustomerCommand(id), cancellationToken);
        return Ok(new { id = customerId, status = "Inactive" });
    }

    #endregion
}

#endregion
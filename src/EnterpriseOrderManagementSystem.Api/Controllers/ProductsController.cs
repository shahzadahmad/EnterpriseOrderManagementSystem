
using EnterpriseOrderManagementSystem.API.Contracts.Products;
using EnterpriseOrderManagementSystem.Application.Common.Models;
using EnterpriseOrderManagementSystem.Application.DTOs.Products;
using EnterpriseOrderManagementSystem.Application.Features.Products.Commands;
using EnterpriseOrderManagementSystem.Application.Features.Products.Queries;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseOrderManagementSystem.API.Controllers;

[Route("api/[controller]")]
public sealed class ProductsController : BaseApiController
{
    public ProductsController(MediatR.ISender sender)
        : base(sender)
    {
    }

    #region Create Product

    /// <summary>Creates a new product.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateProductCommand(
            request.SKU,
            request.Name,
            request.Description,
            request.Price,
            request.Currency);

        var productId = await Sender.Send(
            command,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = productId },
            new { id = productId });
    }

    #endregion

    #region Get Product By Id

    /// <summary>Retrieves a product by its identifier.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var product = await Sender.Send(
            new GetProductByIdQuery(id),
            cancellationToken);

        if (product is null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    #endregion

    #region Get Products

    /// <summary>Retrieves products using pagination.</summary>
    [HttpGet]
    [ProducesResponseType(
        typeof(PaginatedResult<ProductDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedResult<ProductDto>>> GetAll(
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

        var pagination = new PaginationRequest(
            pageNumber,
            pageSize);

        var products = await Sender.Send(
            new GetProductsQuery(pagination),
            cancellationToken);

        return Ok(products);
    }

    #endregion

    #region Update Product

    /// <summary>Updates a product's name and description.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var productId = await Sender.Send(
            new UpdateProductCommand(
                id,
                request.Name,
                request.Description),
            cancellationToken);

        return Ok(new
        {
            id = productId,
            message = "Product details updated successfully."
        });
    }

    #endregion

    #region Change Product Price

    /// <summary>Changes a product's price and currency.</summary>
    [HttpPatch("{id:guid}/price")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePrice(
        [FromRoute] Guid id,
        [FromBody] ChangeProductPriceRequest request,
        CancellationToken cancellationToken)
    {
        var productId = await Sender.Send(
            new ChangeProductPriceCommand(
                id,
                request.Price,
                request.Currency),
            cancellationToken);

        return Ok(new
        {
            id = productId,
            message = "Product price changed successfully."
        });
    }

    #endregion

    #region Activate Product

    /// <summary>Activates an existing product.</summary>
    [HttpPatch("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Activate(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var productId = await Sender.Send(
            new ActivateProductCommand(id),
            cancellationToken);

        return Ok(new
        {
            id = productId,
            status = "Active"
        });
    }

    #endregion

    #region Deactivate Product

    /// <summary>Deactivates an existing product.</summary>
    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Deactivate(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var productId = await Sender.Send(
            new DeactivateProductCommand(id),
            cancellationToken);

        return Ok(new
        {
            id = productId,
            status = "Inactive"
        });
    }

    #endregion

    #region Discontinue Product

    /// <summary>Discontinues an existing product.</summary>
    [HttpPatch("{id:guid}/discontinue")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Discontinue(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var productId = await Sender.Send(
            new DiscontinueProductCommand(id),
            cancellationToken);

        return Ok(new
        {
            id = productId,
            status = "Discontinued"
        });
    }

    #endregion
}

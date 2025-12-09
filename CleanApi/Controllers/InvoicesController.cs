using Microsoft.AspNetCore.Mvc;
using CleanApi.Commands;
using CleanApi.Models;
using CleanApi.Responses;

namespace CleanApi.Controllers;

/// <summary>
/// Controller for invoice operations
/// Inherits from ApiControllerBase for Mediator access
/// </summary>
public class InvoicesController : ApiControllerBase
{
    /// <summary>
    /// Retrieves invoice reference codes
    /// </summary>
    /// <param name="count">Number of RefCodes to return (1-100, default: 3)</param>
    /// <returns>List of invoice reference codes</returns>
    [HttpGet]
    [ProducesResponseType(typeof(GetInvoicesResponse), StatusCodes.Status200OK)]
    public async Task<GetInvoicesResponse> GetInvoices([FromQuery] int? count)
    {
        return await Mediator.Send(new GetInvoicesCommand(count));
    }

    /// <summary>
    /// Retrieves a single invoice by reference code
    /// </summary>
    /// <param name="refCode">The reference code of the invoice</param>
    /// <returns>The invoice if found</returns>
    [HttpGet("{refCode}")]
    [ProducesResponseType(typeof(GetInvoiceByRefCodeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(GetInvoiceByRefCodeResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInvoiceByRefCode([FromRoute] string refCode)
    {
        var result = await Mediator.Send(new GetInvoiceByRefCodeCommand(refCode));

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Creates a new invoice
    /// </summary>
    /// <param name="request">The create invoice request</param>
    /// <returns>Success or failure with validation errors</returns>
    [HttpPost]
    [ProducesResponseType(typeof(InvoiceResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(InvoiceResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(InvoiceResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateInvoiceRequest request)
    {
        var result = await Mediator.Send(new CreateInvoiceCommand(request.PayerName, request.Invoice));

        if (!result.Success)
        {
            if (result.Reason?.Contains("already exists") == true)
            {
                return Conflict(result);
            }
            return BadRequest(result);
        }

        return CreatedAtAction(
            nameof(GetInvoiceByRefCode),
            new { refCode = request.Invoice.RefCode },
            result);
    }

    /// <summary>
    /// Updates invoices (upsert behavior - creates or updates)
    /// </summary>
    /// <param name="request">The invoice request containing invoices to update</param>
    /// <returns>Success or failure with validation errors</returns>
    [HttpPut]
    [ProducesResponseType(typeof(InvoiceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(InvoiceResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update([FromBody] InvoiceRequest request)
    {
        var result = await Mediator.Send(new UpdateInvoiceCommand(request.PayerName, request.Invoices));

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Deletes an invoice by reference code
    /// </summary>
    /// <param name="refCode">The reference code of the invoice to delete</param>
    /// <returns>Success or failure</returns>
    [HttpDelete("{refCode}")]
    [ProducesResponseType(typeof(InvoiceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(InvoiceResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] string refCode)
    {
        var result = await Mediator.Send(new DeleteInvoiceCommand(refCode));

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}

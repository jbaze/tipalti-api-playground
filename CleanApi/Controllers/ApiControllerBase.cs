using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanApi.Controllers;

/// <summary>
/// Base controller that provides MediatR access for all API controllers
/// </summary>
[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _mediator;

    /// <summary>
    /// Gets the MediatR sender for dispatching commands and queries
    /// </summary>
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();
}

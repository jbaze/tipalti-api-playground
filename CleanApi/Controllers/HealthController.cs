using Microsoft.AspNetCore.Mvc;
using CleanApi.Responses;
using Serilog;

namespace CleanApi.Controllers;

/// <summary>
/// Controller for health check endpoints
/// </summary>
public class HealthController : ApiControllerBase
{
    /// <summary>
    /// Health check endpoint for monitoring and load balancers
    /// </summary>
    /// <returns>Current health status of the API</returns>
    [HttpGet("/health")]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    public HealthResponse GetHealth()
    {
        Log.Information("Health check requested");

        return new HealthResponse
        {
            Status = "healthy",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0"
        };
    }
}

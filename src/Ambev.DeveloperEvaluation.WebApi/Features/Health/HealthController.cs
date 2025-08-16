using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Health;

/// <summary>
/// Controller for health check operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Health")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Check the health status of the API
    /// </summary>
    /// <returns>Health status information</returns>
    /// <response code="200">API is healthy and running</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(HealthResponse), 200)]
    public IActionResult GetHealth()
    {
        return Ok(new HealthResponse
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Service = "Ambev Developer Evaluation API",
            Version = "1.0.0"
        });
    }
}

public class HealthResponse
{
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Service { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}
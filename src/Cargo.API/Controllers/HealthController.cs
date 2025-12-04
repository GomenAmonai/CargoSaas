using Microsoft.AspNetCore.Mvc;

namespace Cargo.API.Controllers;

/// <summary>
/// Контроллер для проверки здоровья приложения (healthcheck)
/// </summary>
[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Проверка здоровья API
    /// </summary>
    /// <returns>Статус здоровья</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        _logger.LogDebug("Health check called");
        
        return Ok(new
        {
            status = "Healthy",
            timestamp = DateTime.UtcNow,
            service = "Cargo.API",
            version = "1.0.0"
        });
    }
}



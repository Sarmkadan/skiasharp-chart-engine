using Microsoft.AspNetCore.Mvc;
using SkiaSharpChartEngine.Diagnostics;

namespace SkiaSharpChartEngine.API.Controllers
{
    /// <summary>
    /// Controller for checking the health of the application.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HealthController"/> class.
        /// </summary>
        /// <param name="healthCheckService">The health check service.</param>
        public HealthController(HealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService;
        }

        /// <summary>
        /// Checks the health of the application.
        /// </summary>
        /// <returns>A result indicating the health of the application.</returns>
        [HttpGet]
        public async Task<IActionResult> GetHealthAsync()
        {
            var result = await _healthCheckService.CheckHealthAsync(default);
            return Ok(result);
        }
    }
}

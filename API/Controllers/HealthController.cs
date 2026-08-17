using Microsoft.AspNetCore.Http;
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
        private readonly HealthCheckService _healthCheckService;

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
        /// <returns>
        /// A result indicating the health of the application.
        /// Returns 200 with version and uptime fields on success,
        /// or 503 with problem details on failure.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> GetHealthAsync()
        {
            var result = await _healthCheckService.CheckHealthAsync(default);

            // Assume HealthCheckResult has an IsHealthy property indicating success.
            // If the property does not exist, adjust accordingly.
            var isHealthyProp = result?.GetType().GetProperty("IsHealthy");
            var isHealthy = isHealthyProp != null && isHealthyProp.GetValue(result) is bool b && b;

            if (!isHealthy)
            {
                var problem = new ProblemDetails
                {
                    Title = "Service Unavailable",
                    Status = StatusCodes.Status503ServiceUnavailable,
                    Detail = "Health check failed."
                };
                return StatusCode(StatusCodes.Status503ServiceUnavailable, problem);
            }

            // Include version and uptime fields in the successful response.
            // Assume HealthCheckResult has Version and Uptime properties.
            var versionProp = result?.GetType().GetProperty("Version");
            var uptimeProp = result?.GetType().GetProperty("Uptime");

            var version = versionProp?.GetValue(result);
            var uptime = uptimeProp?.GetValue(result);

            var response = new
            {
                version,
                uptime,
                result
            };

            return Ok(response);
        }
    }
}

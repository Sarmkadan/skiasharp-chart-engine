using Microsoft.AspNetCore.Mvc;
using SkiaSharpChartEngine.Diagnostics;

namespace SkiaSharpChartEngine.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly HealthCheckService _healthCheckService;

        public HealthController(HealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService;
        }

        [HttpGet]
        public async Task<IActionResult> GetHealthAsync()
        {
            var result = await _healthCheckService.CheckHealthAsync(default);
            return Ok(result);
        }
    }
}
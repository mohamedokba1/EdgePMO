using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EdgePMO.API.Controllers
{
    [Route("v1.0/[controller]")]
    [ApiController]
    public class SystemController : ControllerBase
    {
        private readonly HealthCheckService _healthCheckService;

        public SystemController(HealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService;
        }

        [HttpGet("health")]
        public async Task<IActionResult> GetHealth()
        {
            HealthReport? report = await _healthCheckService.CheckHealthAsync();

            var response = new
            {
                status = report.Status.ToString()
            };

            if (report.Status == HealthStatus.Healthy)
                return Ok(response);

            return StatusCode(503, response);
        }
    }
}

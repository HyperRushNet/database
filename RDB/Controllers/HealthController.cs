using Microsoft.AspNetCore.Mvc;

namespace RDB.Controllers;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult CheckHealth()
    {
        long uptimeSeconds = Environment.TickCount64 / 1000;

        var uptime = TimeSpan.FromSeconds(uptimeSeconds);
        string humanReadable = $"{(int)uptime.TotalDays}d {uptime.Hours}h {uptime.Minutes}m";

        return Ok(new {
            status = "Healthy",
            uptimeSeconds,
            uptime = humanReadable
        });
    }
}

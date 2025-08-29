using Microsoft.AspNetCore.Mvc;

namespace RDB.Controllers;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult CheckHealth()
    {
        return Ok(new {
            status = "Healthy",
            uptime = Environment.TickCount64 / 1000 
        });
    }
}

using Microsoft.AspNetCore.Mvc;

namespace RDB.Controllers;

[ApiController]
[Route("info")]
public class InfoController : ControllerBase
{
    [HttpGet]
    public IActionResult GetInfo()
    {
        return Ok(new {
            service = "RDB",
            version = "1.0.0",
            timeUtc = DateTime.UtcNow
        });
    }
}

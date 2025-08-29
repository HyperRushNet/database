using Microsoft.AspNetCore.Mvc;

namespace RDB.Controllers;

[ApiController]
[Route("ping")]
public class PingController : ControllerBase
{
    [HttpGet]
    public IActionResult Ping() => Ok();
}

using Microsoft.AspNetCore.Mvc;
using RDB.Services;

namespace RDB.Controllers;

[ApiController]
[Route("stats")]
public class StatsController : ControllerBase
{
    private readonly IStorageService _db;

    public StatsController(IStorageService db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetStats([FromQuery] string type)
    {
        var items = await _db.GetAllItemsAsync(type);
        return Ok(new {
            type,
            count = items.Count
        });
    }
}

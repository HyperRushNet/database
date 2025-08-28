using Microsoft.AspNetCore.Mvc;
using RDB.Services;
using RDB.Models;

namespace RDB.Controllers;

[ApiController]
[Route("[controller]")]
public class DatabaseController : ControllerBase
{
    private readonly DatabaseService _db;
    public DatabaseController(DatabaseService db) => _db = db;

    [HttpPost]
    public IActionResult AddItem([FromQuery] string type, [FromBody] object payload)
    {
        var item = _db.AddItem(type, payload ?? new {});
        return new JsonResult(new { id = item.Id });
    }

    [HttpGet("item")]
    public IActionResult GetItem([FromQuery] string type, [FromQuery] string id, [FromQuery] bool raw = false)
    {
        var item = _db.GetItem(type, id);
        if (item == null) return StatusCode(204);
        return raw ? new JsonResult(item) : new JsonResult(new { item.Id });
    }

    [HttpGet("items")]
    public IActionResult GetItems([FromQuery] string type)
    {
        var items = _db.GetItems(type).Select(i => new { i.Id });
        return new JsonResult(items);
    }

    [HttpDelete("item")]
    public IActionResult DeleteItem([FromQuery] string type, [FromQuery] string id)
    {
        var success = _db.DeleteItem(type, id);
        return success ? new JsonResult(new { id }) : StatusCode(204);
    }
}

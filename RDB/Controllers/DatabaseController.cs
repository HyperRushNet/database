using Microsoft.AspNetCore.Mvc;
using RDB.Models;
using RDB.Services;

namespace RDB.Controllers;

[ApiController]
[Route("[controller]")]
public class DatabaseController : ControllerBase
{
    private readonly IStorageService _db;

    public DatabaseController(IStorageService db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> AddItem([FromQuery] string type, [FromBody] object payload)
    {
        var item = new ItemEnvelope
        {
            Id = Guid.NewGuid().ToString("N"),
            Type = type,
            CreatedAt = DateTime.UtcNow,
            Payload = payload
        };
        await _db.SaveItemAsync(item);
        return Ok(item);
    }

    [HttpGet("item")]
    public async Task<IActionResult> GetItem([FromQuery] string id, [FromQuery] string type)
    {
        var item = await _db.GetItemAsync(type, id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpGet("items")]
    public async Task<IActionResult> GetAll([FromQuery] string type)
    {
        var list = await _db.GetAllItemsAsync(type);
        return Ok(list);
    }
}

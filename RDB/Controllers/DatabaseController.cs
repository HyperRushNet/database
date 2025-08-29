using Microsoft.AspNetCore.Mvc;
using RDB.Models;
using RDB.Services;

namespace RDB.Controllers
{
    [ApiController]
    [Route("database")]
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
        public async Task<IActionResult> GetItem([FromQuery] string type, [FromQuery] string id)
        {
            var item = await _db.GetItemAsync(type, id);
            if(item == null) return NotFound();
            return Ok(item);
        }

        [HttpGet("items")]
        public async Task<IActionResult> GetAll([FromQuery] string type, [FromQuery] int skip = 0, [FromQuery] int take = 100)
        {
            var list = await _db.GetAllItemsAsync(type, skip, take);
            return Ok(list);
        }

        [HttpDelete("item")]
        public async Task<IActionResult> DeleteItem([FromQuery] string type, [FromQuery] string id)
        {
            var success = await _db.DeleteItemAsync(type, id);
            if (!success) return NotFound();
            return Ok(new { deleted = true });
        }
    }
}

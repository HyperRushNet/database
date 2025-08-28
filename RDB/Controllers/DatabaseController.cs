using Microsoft.AspNetCore.Mvc;
using RDB.Services;
using RDB.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RDB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseController : ControllerBase
    {
        private readonly DatabaseService _databaseService;

        public DatabaseController(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        [HttpGet("{type}")]
        public async Task<ActionResult<List<ItemEnvelope>>> GetAll(string type)
        {
            var items = await _databaseService.GetAllItems(type);
            return Ok(items);
        }

        [HttpGet("{type}/{id}")]
        public async Task<ActionResult<ItemEnvelope>> Get(string type, string id)
        {
            var item = await _databaseService.GetItem(type, id);
            if (item == null)
                return NotFound();
            return Ok(item);
        }

        [HttpPost("{type}")]
        public async Task<ActionResult<ItemEnvelope>> Add(string type, [FromBody] Dictionary<string, object> payload)
        {
            if (payload == null)
                return BadRequest("Payload cannot be null.");

            var addedItem = await _databaseService.AddItem(type, payload);
            return Ok(addedItem);
        }

        [HttpDelete("{type}/{id}")]
        public async Task<IActionResult> Remove(string type, string id)
        {
            var removed = await _databaseService.RemoveItem(type, id);
            if (!removed)
                return NotFound();
            return NoContent();
        }
    }
}

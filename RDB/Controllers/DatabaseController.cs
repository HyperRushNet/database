using Microsoft.AspNetCore.Mvc;
using RDB.Services;
using RDB.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RDB.Controllers
{
    [ApiController]
    [Route("database")]
    public class DatabaseController : ControllerBase
    {
        private readonly DatabaseService _dbService;

        public DatabaseController(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        [HttpPost]
        public async Task<Models.ItemEnvelope> AddItem([FromQuery] string type, [FromBody] Dictionary<string, object> payload)
        {
            return await _dbService.AddItem(type, payload);
        }

        [HttpGet("items")]
        public async Task<List<Models.ItemEnvelope>> GetItems([FromQuery] string type)
        {
            return await _dbService.GetItems(type);
        }

        [HttpGet("item")]
        public async Task<Models.ItemEnvelope> GetItem([FromQuery] string type, [FromQuery] string id)
        {
            return await _dbService.GetItem(type, id);
        }

        [HttpDelete("item")]
        public async Task<bool> DeleteItem([FromQuery] string type, [FromQuery] string id)
        {
            return await _dbService.DeleteItem(type, id);
        }
    }
}

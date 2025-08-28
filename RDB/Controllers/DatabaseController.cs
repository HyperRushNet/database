using Microsoft.AspNetCore.Mvc;
using RDB.Services;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RDB.Controllers
{
    [ApiController]
    [Route("/")]
    public class DatabaseController : ControllerBase
    {
        private readonly IStorageService _storage;
        public DatabaseController(IStorageService storage){_storage=storage;}
        [HttpGet] public ActionResult Welcome() => Ok(new { message="Database API - use /{type} endpoints."});
        [HttpGet("{type}")] public async Task<IActionResult> ListType([FromRoute]string type,[FromQuery]int skip=0,[FromQuery]int take=100){take=Math.Min(500,Math.Max(1,take)); var list = await _storage.ListTypeAsync(type,skip,take); return Ok(list);}
        [HttpGet("{type}/{id}")] public async Task<IActionResult> GetItem([FromRoute]string type,[FromRoute]string id){var item=await _storage.GetItemAsync(type,id);if(item==null) return NotFound(); return Ok(item);}
        [HttpPost("{type}")] public async Task<IActionResult> AddItem([FromRoute]string type){using var doc=await JsonDocument.ParseAsync(Request.Body); var payload=doc.RootElement.Clone(); var entry=await _storage.StoreItemAsync(type,payload); return Created($"/{type}/{entry.Id}",entry);}
        [HttpPost("{type}/batch")] public async Task<IActionResult> AddBatch([FromRoute]string type){using var doc=await JsonDocument.ParseAsync(Request.Body); if(doc.RootElement.ValueKind!=JsonValueKind.Array) return BadRequest(new { error="Batch payload must be JSON array"}); var payloads=new List<JsonElement>(); foreach(var el in doc.RootElement.EnumerateArray()) payloads.Add(el.Clone()); var results=await _storage.StoreBatchAsync(type,payloads); return Created($"/{type}",results);}
    }
}

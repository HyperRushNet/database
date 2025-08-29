[HttpGet("items")]
public async Task<IActionResult> GetAll([FromQuery] string type, [FromQuery] int skip = 0, [FromQuery] int take = 100)
{
    var list = await _db.GetAllItemsAsync(type, skip, take);
    return Ok(list);
}

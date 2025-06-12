using Microsoft.AspNetCore.Mvc;
using T3.Clone.Server.Service;

namespace T3.Clone.Server.Controller;

[Route("api/[controller]")]
public class AiKeyController(
    AiKeyService service
) : ControllerBase
{
    [HttpGet]
    public IActionResult GetKeys()
    {
        return Ok(service.GetKeys());
    }
    
    [HttpPost("{identifier}")]
    public IActionResult AddKey(string identifier, [FromBody] string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return BadRequest("API key cannot be empty.");
        }

        service.AddKey(identifier, key);
        return Ok();
    }
}
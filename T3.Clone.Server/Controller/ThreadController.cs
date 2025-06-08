using Microsoft.AspNetCore.Mvc;
using T3.Clone.Server.Service;

namespace T3.Clone.Server.Controller;

[Route("api/[controller]")]
public class ThreadController(
    ThreadService service
    ) : ControllerBase
{
    
    [HttpGet("threads")]
    public async Task<IActionResult> GetThreads([FromQuery] int? clientVersion)
    {
        var threads = await service.GetThreads(clientVersion);
        return Ok(threads);
    }
}
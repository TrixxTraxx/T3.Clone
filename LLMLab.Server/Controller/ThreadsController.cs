using LLMLab.Dtos.Threads;
using LLMLab.Server.Service;
using Microsoft.AspNetCore.Mvc;

namespace LLMLab.Server.Controller;

[Route("api/[controller]")]
public class ThreadsController(
    ThreadService service
    ) : ControllerBase
{
    
    [HttpGet()]
    public async Task<IActionResult> GetThreads([FromQuery] int? clientVersion)
    {
        var threads = await service.GetThreads(clientVersion);
        return Ok(threads);
    }
    
    [HttpPut]
    public async Task<IActionResult> UpdateThread([FromBody] ThreadDto threadDto)
    {
        var updatedThread = await service.UpdateThread(threadDto);
        if (updatedThread == null)
        {
            return NotFound();
        }
        //dont return anything, client should trigger the sync
        return Ok();
    }
}
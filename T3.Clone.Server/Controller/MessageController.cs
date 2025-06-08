using Microsoft.AspNetCore.Mvc;
using T3.Clone.Dtos.Messages;
using T3.Clone.Server.Data;
using T3.Clone.Server.Service;

namespace T3.Clone.Server.Controller;

[Route("api/[controller]")]
public class MessageController(
    MessageService service
) : ControllerBase
{
    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] MessageDto message)
    {
        var result = await service.CreateMessageAsync(message);
        return Ok(result);
    }
}
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
    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] MessageDto message)
    {
        var result = await service.CreateMessageAsync(message);
        return Ok(result);
    }
    
    [HttpGet("{messageId}")]
    public async Task<IActionResult> GetMessage([FromQuery] int messageId)
    {
        var messages = await service.GetMessageAsync(messageId);
        return Ok(messages);
    }
}
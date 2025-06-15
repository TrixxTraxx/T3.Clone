using LLMLab.Dtos.Messages;
using LLMLab.Server.Service;
using Microsoft.AspNetCore.Mvc;

namespace LLMLab.Server.Controller;

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
    public async Task<IActionResult> GetMessage(int messageId)
    {
        var messages = await service.GetMessageAsync(messageId);
        return Ok(messages);
    }
}
using System.Security.Claims;
using LLMLab.Server.Data;
using LLMLab.Server.Service;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace LLMLab.Server.SignalR;

public class MessageHub(
    ApplicationDbContext db,
    AiGenerationService aiGenerationService
) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.GetHttpContext()!.Request.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            Context.Abort();
            return;
        }

        var user = await db.Users
            .SingleOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            Context.Abort();
            return;
        }
        
        //we technically dont need the User, we just need to know he owns the message
        //Context.Items["User"] = user;
        
        // Get the Thread Id
        var messageId = Context.GetHttpContext()!.Request.Query["messageId"].ToString();
        if (string.IsNullOrEmpty(messageId))
        {
            Context.Abort();
            return;
        }
        Context.Items["MessageId"] = messageId;
        
        //Set Group for the message
        await Groups.AddToGroupAsync(Context.ConnectionId, messageId);

        await aiGenerationService.SendExistingMessage(user, int.Parse(messageId), Context);

        await base.OnConnectedAsync();
    }
    
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        // Handle disconnection logic if needed
        return base.OnDisconnectedAsync(exception);
    }
    
    public async Task StopGeneration()
    {
        var messageId = Context.Items["MessageId"]!.ToString();

        // Call the service to stop generation
        await aiGenerationService.StopGeneration(int.Parse(messageId));
    }
}
using System.Security.Claims;
using LLMLab.Server.Data;
using LLMLab.Server.Service;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace LLMLab.Server.SignalR;

public class ThreadHub(
    ApplicationDbContext db
) : Hub
{
    //this hub is for messaging that a thread update has occoured
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
        
        Context.Items["User"] = user;
        
        //Set Group for the message
        await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        
        await base.OnConnectedAsync();
    }
}
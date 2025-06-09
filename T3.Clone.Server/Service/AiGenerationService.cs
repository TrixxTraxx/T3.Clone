using Hangfire;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using T3.Clone.Server.Data;
using T3.Clone.Server.Jobs;
using T3.Clone.Server.SignalR;

namespace T3.Clone.Server.Service;

public class AiGenerationService(
    ApplicationDbContext dbContext,
    IHubContext<MessageHub> hubContext
)
{
    public async Task StartGeneration(int messageId)
    {
        BackgroundJob.Enqueue<GenerateMessageJob>(x => x.GenerateMessageAsync(messageId));
    }

    public async Task GenerateThreadTitle(int threadId, string newMessageText)
    {
        BackgroundJob.Enqueue<GenerateThreadTitleJob>(x => x.GenerateThreadTitleAsync(threadId, newMessageText));
    }

    public async Task StopGeneration(int messageId)
    {
        throw new NotImplementedException();
    }

    public async Task AddTokenToGeneration(int messageId, string token)
    {
        var message = await dbContext.Messages.FindAsync(messageId);
        if (message == null)
        {
            throw new ArgumentException("Message not found", nameof(messageId));
        }
        message.ModelResponse += token;
        await dbContext.SaveChangesAsync();
        SendNewToken(messageId, token);
    }

    public void SendExistingMessage(int messageId, HubCallerContext context)
    {
        var message = dbContext.Messages.Find(messageId);
        if (message == null)
        {
            throw new ArgumentException("Message not found", nameof(messageId));
        }
        
        // Send the existing message to the client
        hubContext.Clients.Client(context.ConnectionId).SendAsync("NewMessage", message);
    }
    
    public void SendNewToken(int messageId, string token)
    {
        // get all Clients for the messageId
        var clients = hubContext.Clients.Group(messageId.ToString());
        if (clients == null)
        {
            throw new ArgumentException("No clients found for the given messageId", nameof(messageId));
        }
        
        // Send the new token to all clients in the group
        clients.SendAsync("ReceiveNewToken", token).GetAwaiter().GetResult();
    }
}
using Hangfire;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using T3.Clone.Server.Data;
using T3.Clone.Server.Jobs;
using T3.Clone.Server.Mappers;
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
        var message = await dbContext.Messages
            .Include(x => x.Attachments)
            .FirstOrDefaultAsync(x => x.Id == messageId);
        if (message == null)
        {
            throw new ArgumentException("Message not found", nameof(messageId));
        }
        
        // Mark the message as complete
        message.Complete = true;
        await dbContext.SaveChangesAsync();
        
        // Notify clients that the generation has stopped
        await hubContext.Clients.Group(messageId.ToString()).SendAsync("GenerationStopped", MessageMapper.Map(message));
    }

    public async Task AddTokenToGeneration(int messageId, string token, bool isThinkingToken)
    {
        var message = await dbContext.Messages.FindAsync(messageId);
        if (message == null)
        {
            throw new ArgumentException("Message not found", nameof(messageId));
        }
        message.ModelResponse += token;
        await dbContext.SaveChangesAsync();
        SendNewToken(messageId, token, isThinkingToken);
    }

    public async Task SendExistingMessage(ApplicationUser user, int messageId, HubCallerContext context)
    {
        var message = await dbContext.Messages
            .Include(x => x.Attachments)
            .FirstOrDefaultAsync(x => x.Id == messageId && x.Thread.UserId == user.Id);
        if (message == null)
        { 
            throw new ArgumentException("Message with Id not found", nameof(messageId));
        }
        
        
        if (message.Complete || message.CreatedAt.AddMinutes(5) < DateTime.UtcNow)
        {
            // If the message is complete, notify the client
            await StopGeneration(messageId);
            return;
        }
        
        // Send the existing message to the client
        await hubContext.Clients.Client(context.ConnectionId).SendAsync("NewMessage", MessageMapper.Map(message));
    }
    
    public void SendNewToken(int messageId, string token, bool isThinkingToken)
    {
        // get all Clients for the messageId
        var clients = hubContext.Clients.Group(messageId.ToString());
        if (clients == null)
        {
            throw new ArgumentException("No clients found for the given messageId", nameof(messageId));
        }
        
        // Send the new token to all clients in the group
        clients.SendAsync("ReceiveNewToken", token, isThinkingToken).GetAwaiter().GetResult();
    }
}
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using T3.Clone.Server.Data;
using T3.Clone.Server.SignalR;

namespace T3.Clone.Server.Service;

public class AiGenerationService(
    ApplicationDbContext dbContext,
    IHubContext<MessageHub> hubContext,
    IServiceProvider serviceProvider
)
{
    public async Task StartGeneration(int messageId)
    {
        _ = Task.Run((Action) (async () => {
            try
            {
                //placeholder, hard code response
                var message = await dbContext.Messages.FindAsync(messageId);
                if (message == null)
                {
                    throw new ArgumentException("Message not found", nameof(messageId));
                }
                message.ModelResponse = "This is a generated response for the message.";
                message.Complete = true;
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Handle exceptions from AI generation
                Console.WriteLine($"AI Generation failed: {ex.Message}");
            }
        }));
    }

    public async Task GenerateThreadTitle(int threadId, string newMessageText)
    {
        _ = Task.Run(async () => {
            try
            {
                // Placeholder for generating a thread title based on the new message text
                var thread = await dbContext.MessageThreads
                    .Include(x => x.User)
                    .FirstOrDefaultAsync(t => t.Id == threadId);
                if (thread == null)
                {
                    throw new ArgumentException("Thread not found", nameof(threadId));
                }

                thread.User.ThreadVersion++;
                thread.Version = thread.User.ThreadVersion;
                // Simulate title generation
                thread.Title = $"Generated Title: {newMessageText.Substring(0, Math.Min(newMessageText.Length, 50))}";
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Handle exceptions from AI generation
                Console.WriteLine($"AI Thread Title generation failed: {ex.Message}");
            }
        });
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
        clients.SendAsync("ReceiveNewToken", messageId, token).GetAwaiter().GetResult();
    }
}
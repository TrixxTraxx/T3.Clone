using Microsoft.EntityFrameworkCore;
using T3.Clone.Client.Services;
using T3.Clone.Dtos.Messages;
using T3.Clone.Server.Data;
using T3.Clone.Server.Service;

namespace T3.Clone.Server.Jobs;

public class GenerateMessageJob(
    ApplicationDbContext dbContext,
    AiGenerationService aiGenerationService,
    ChatModelProvider chatModelProvider
)
{
    public async Task GenerateMessageAsync(int messageId) 
    {
        var message = await dbContext.Messages
            .Include(x => x.Attachments)
            .Include(x => x.Model)
            .FirstOrDefaultAsync(x => x.Id == messageId);
        if (message == null)
        {
            throw new ArgumentException("Message not found", nameof(messageId));
        }

        message.ModelResponse = string.Empty;
        await dbContext.SaveChangesAsync();
        
        var messageChain = GetMessageChain(message);
        
        var model = chatModelProvider.GetChatModel(message.Model);
        var result = await model.GenerateResponse(
            message,
            messageChain,
            message.Model,
            token => aiGenerationService.SendNewToken(messageId, token),
            error =>
            {
                //TODO: handle error
                Console.WriteLine($"Error during token generation: {error}");
                aiGenerationService.StopGeneration(messageId);
            }
        );
        
        await dbContext.SaveChangesAsync();
        
        await SaveResultAsync(messageId, result);
        
        await aiGenerationService.StopGeneration(messageId);
    }

    private async Task SaveResultAsync(int messageId, ChatModelResponse result)
    {
        //TODO: Implement saving the result of the chat model response and Token Metadata
    }

    private List<Message> GetMessageChain(Message message)
    {
        var messages = dbContext.Messages
            .Include(x => x.Attachments)
            .Where(x => x.ThreadId == message.ThreadId && x.Id != message.Id)
            .OrderBy(x => x.CreatedAt)
            .ToList();
        
        var chain = new List<Message>();
        var currentMessage = message;
        while (currentMessage != null)
        {
            chain.Add(currentMessage);
            if (currentMessage.PreviousMessageId == 0)
            {
                break; // No previous message, end of chain
            }
            currentMessage = messages.FirstOrDefault(x => x.Id == currentMessage.PreviousMessageId);
        }
        
        return chain;
    }
}
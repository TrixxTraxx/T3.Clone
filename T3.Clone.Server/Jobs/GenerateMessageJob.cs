using Hangfire;
using Microsoft.EntityFrameworkCore;
using T3.Clone.Client.Services;
using T3.Clone.Server.Data;
using T3.Clone.Server.Service;

namespace T3.Clone.Server.Jobs;

[AutomaticRetry(Attempts = 0)]
public class GenerateMessageJob(
    ApplicationDbContext dbContext,
    AiGenerationService aiGenerationService,
    ChatModelProvider chatModelProvider
)
{
    public async Task GenerateMessageAsync(int messageId) 
    {
        Message? message = null;
        try
        {
            message = await dbContext.Messages
                .Include(x => x.Thread)
                .Include(x => x.Attachments)
                .Include(x => x.Model)
                .FirstOrDefaultAsync(x => x.Id == messageId);
            
            if (message == null)
            {
                Console.WriteLine($"Message with ID {messageId} not found");
                return;
            }

            message.ModelResponse = string.Empty;
            await dbContext.SaveChangesAsync();
            
            var messageChain = GetMessageChain(message);
            
            var model = chatModelProvider.GetChatModel(message.Model);
            Console.WriteLine($"Using model: {message.Model.ModelId} for message {messageId} with provider {model.GetType().Name}");
            
            var result = await model.GenerateResponse(
                message,
                messageChain,
                message.Model,
                token =>
                {
                    SaveDebounced();
                    aiGenerationService.SendNewToken(messageId, token, false);
                },
                thinkingToken =>
                {
                    SaveDebounced();
                    aiGenerationService.SendNewToken(messageId, thinkingToken, true);
                },
                error =>
                {
                    message.Complete = true;
                    message.Error = true;
                    message.ErrorMessage = error;
                    dbContext.SaveChanges();
                    Task.Run(async () => {
                        try
                        {
                            await aiGenerationService.StopGeneration(messageId);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to send error notification for message {messageId}: {ex.Message}");
                        }
                    });
                }
            );
            
            await Task.Delay(50);
            // Mark message as complete
            message.Complete = true;
            await dbContext.SaveChangesAsync();
            
            await SaveResultAsync(messageId, result);
            
            // Only stop generation if there were no errors
            if (result != null && !result.IsError)
            {
                await aiGenerationService.StopGeneration(messageId);
            }
            else
            {
                // Handle error case
                Console.WriteLine($"Message generation failed for message {messageId}: {result?.ErrorMessage}");
                await aiGenerationService.StopGeneration(messageId);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error in GenerateMessageAsync for message {messageId}: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            
            // Mark message as complete even if there was an error to prevent infinite retries
            try
            {
                message!.Complete = true;
                message.ModelResponse = $"Error: {ex.Message}";
                await dbContext.SaveChangesAsync();
            }
            catch (Exception saveEx)
            {
                Console.WriteLine($"Failed to save error state for message {messageId}: {saveEx.Message}");
            }
            
            try
            {
                await aiGenerationService.StopGeneration(message!.Id);
            }
            catch (Exception notifyEx)
            {
                Console.WriteLine($"Failed to send error notification for message {messageId}: {notifyEx.Message}");
            }
        }
    }

    private DateTime lastSaveTime = DateTime.MinValue;
    private void SaveDebounced()
    {
        var now = DateTime.UtcNow;
        if ((now - lastSaveTime).TotalSeconds < 1)
        {
            // If last save was less than 1 second ago, skip this save
            return;
        }
        
        lastSaveTime = now; 
        dbContext.SaveChanges();
    }

    private async Task SaveResultAsync(int messageId, ChatModelResponse? result)
    {
        if (result == null)
        {
            Console.WriteLine($"No result to save for message {messageId}");
            return;
        }

        try
        {
            var message = await dbContext.Messages.FirstOrDefaultAsync(m => m.Id == messageId);
            if (message == null)
            {
                Console.WriteLine($"Message {messageId} not found when saving result");
                return;
            }

            // Update message with response information if available
            if (!string.IsNullOrEmpty(result.Response) && string.IsNullOrEmpty(message.ModelResponse))
            {
                message.ModelResponse = result.Response;
            }

            // Save token usage and model information if needed
            // TODO: Create a MessageResult or MessageMetadata table to store this information
            Console.WriteLine($"Message {messageId} completed. Provider: {result.ModelProvider}, " +
                            $"Model: {result.ModelId}, Input tokens: {result.InputTokens}, " +
                            $"Output tokens: {result.OutputTokens}, Error: {result.IsError}");

            if (result.IsError)
            {
                Console.WriteLine($"Message {messageId} generation error: {result.ErrorMessage}");
            }

            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving result for message {messageId}: {ex.Message}");
        }
    }

    private List<Message> GetMessageChain(Message message)
    {
        try
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
        catch (Exception ex)
        {
            Console.WriteLine($"Error building message chain for message {message.Id}: {ex.Message}");
            // Return at least the current message if chain building fails
            return new List<Message> { message };
        }
    }
}
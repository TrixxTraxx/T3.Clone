using Hangfire;
using Microsoft.EntityFrameworkCore;
using T3.Clone.Client.Services;
using T3.Clone.Server.Data;

namespace T3.Clone.Server.Jobs;

[AutomaticRetry(Attempts = 0)]
public class GenerateThreadTitleJob(
    ApplicationDbContext dbContext,
    ChatModelProvider chatModelProvider
)
{
    public async Task GenerateThreadTitleAsync(int threadId, string newMessageText)
    {
        MessageThread? thread = null;
        try
        {
            thread = await dbContext.MessageThreads
                .Include(x => x.User)
                .Include(x => x.Messages)
                .ThenInclude(x => x.Model)
                .FirstOrDefaultAsync(x => x.Id == threadId);
            
            if (thread == null)
            {
                Console.WriteLine($"Thread with ID {threadId} not found");
                return;
            }
            
            //there should only be one message in the thread for title generation
            var message = thread.Messages.FirstOrDefault();
            if (message == null)
            {
                Console.WriteLine($"No messages found in thread {threadId} for title generation");
                return;
            }
            
            if (message.Model == null)
            {
                Console.WriteLine($"Message {message.Id} in thread {threadId} has no associated model");
                return;
            }
            
            //untrack the message and model
            dbContext.Entry(message).State = EntityState.Detached;
            dbContext.Entry(message.Model).State = EntityState.Detached;

            var config = message.Model;
            config.SystemPrompt = "Generate a short and Concise title for this Thread that is at most 30 characters long!";
            
            var model = chatModelProvider.GetThreadGenerationModel();
            var result = await model.GenerateResponse(
                message,
                new List<Message>(){message},
                config,
                token => {},
                thinkingToken => {},
                error =>
                {
                    Console.WriteLine($"Error during title generation for thread {threadId}: {error}");
                }
            );

            if (result != null && !result.IsError && !string.IsNullOrEmpty(message.ModelResponse))
            {
                // Successfully generated title
                thread.User.ThreadVersion++;
                thread.Version = thread.User.ThreadVersion;
                thread.Title = message.ModelResponse.Trim();
                
                // Ensure title doesn't exceed 30 characters as requested in system prompt
                if (thread.Title.Length > 30)
                {
                    thread.Title = thread.Title.Substring(0, 30).Trim();
                }
                
                await dbContext.SaveChangesAsync();
                Console.WriteLine($"Successfully generated title for thread {threadId}: '{thread.Title}'");
            }
            else
            {
                // Handle error case
                var errorMessage = result?.ErrorMessage ?? "Failed to generate thread title";
                Console.WriteLine($"Thread title generation failed for thread {threadId}: {errorMessage}");
                
                // Set a default title based on the message text
                var fallbackTitle = newMessageText.Length > 30 
                    ? newMessageText.Substring(0, 27) + "..." 
                    : newMessageText;
                
                thread.User.ThreadVersion++;
                thread.Version = thread.User.ThreadVersion;
                thread.Title = fallbackTitle;
                
                await dbContext.SaveChangesAsync();
                Console.WriteLine($"Set fallback title for thread {threadId}: '{thread.Title}'");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error in GenerateThreadTitleAsync for thread {threadId}: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            
            // Set fallback title if thread was found
            if (thread != null)
            {
                try
                {
                    var fallbackTitle = newMessageText.Length > 30 
                        ? newMessageText.Substring(0, 27) + "..." 
                        : newMessageText;
                    
                    thread.User.ThreadVersion++;
                    thread.Version = thread.User.ThreadVersion;
                    thread.Title = fallbackTitle;
                    
                    await dbContext.SaveChangesAsync();
                    Console.WriteLine($"Set emergency fallback title for thread {threadId}: '{thread.Title}'");
                }
                catch (Exception saveEx)
                {
                    Console.WriteLine($"Failed to save fallback title for thread {threadId}: {saveEx.Message}");
                }
            }
        }
    }
}
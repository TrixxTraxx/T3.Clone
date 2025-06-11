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
        var thread = await dbContext.MessageThreads
            .Include(x => x.User)
            .Include(x => x.Messages)
            .ThenInclude(x => x.Model)
            .FirstOrDefaultAsync(x => x.Id == threadId);
        
        //there should only be one message in the thread for title generation
        var message = thread?.Messages.FirstOrDefault();
        if (message == null)
        {
            throw new ArgumentException("Thread not found", nameof(threadId));
        }
        
        //untrack the message and model
        dbContext.Entry(message).State = EntityState.Detached;
        dbContext.Entry(message.Model).State = EntityState.Detached;

        var config = message.Model;
        config.SystemPrompt = "Generate a short and Concise title for this Thread that is at most 30 characters long!";
        
        var model = chatModelProvider.GetChatModel(message.Model);
        var result = await model.GenerateResponse(
            message,
            new List<Message>(){message},
            config,
            token => {},
            thinkingToken => {},
            error =>
            {
                //TODO: handle error
                Console.WriteLine($"Error during token generation: {error}");
            }
        );

        thread.User.ThreadVersion++;
        thread.Version = thread.User.ThreadVersion;
        thread!.Title = message.ModelResponse;
        
        await dbContext.SaveChangesAsync();
    }
}
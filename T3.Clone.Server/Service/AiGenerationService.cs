using T3.Clone.Server.Data;

namespace T3.Clone.Server.Service;

public class AiGenerationService(
    ApplicationDbContext dbContext
)
{
    public async Task StartGeneration(int messageId)
    {
        //placeholder, hard code response
        var message = await dbContext.Messages.FindAsync(messageId);
        if (message == null)
        {
            throw new ArgumentException("Message not found", nameof(messageId));
        }
        message.ModelResponse = "This is a generated response for the message.";
    }

    public async Task GenerateThreadTitle(int threadId, string newMessageText)
    {
        throw new NotImplementedException();
    }
}
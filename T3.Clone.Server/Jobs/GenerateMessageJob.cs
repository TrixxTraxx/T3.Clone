using T3.Clone.Server.Data;
using T3.Clone.Server.Service;

namespace T3.Clone.Server.Jobs;

public class GenerateMessageJob(
    ApplicationDbContext dbContext,
    AiGenerationService aiGenerationService
)
{
    public async Task GenerateMessageAsync(int messageId) 
    {
        var message = await dbContext.Messages.FindAsync(messageId);
        if (message == null)
        {
            throw new ArgumentException("Message not found", nameof(messageId));
        }

        message.ModelResponse = string.Empty;
        await dbContext.SaveChangesAsync();

        for (int i = 0; i < 50; i++)
        {
            // Simulate a delay for each token generation
            await Task.Delay(100);
            
            //generate 4 characters of random text
            var randomToken = new string(Enumerable.Range(0, 4)
                .Select(_ => (char)('a' + Random.Shared.Next(0, 26)))
                .ToArray());
            await aiGenerationService.AddTokenToGeneration(messageId, randomToken);
        }
        //await aiGenerationService.StopGeneration(messageId);
    }
}
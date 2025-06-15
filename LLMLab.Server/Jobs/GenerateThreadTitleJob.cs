using System.ClientModel;
using Hangfire;
using LLMLab.Server.Configuration;
using LLMLab.Server.Data;
using LLMLab.Server.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace LLMLab.Server.Jobs;

[AutomaticRetry(Attempts = 2, DelaysInSeconds = new[] { 10, 30 })]
public class GenerateThreadTitleJob(
    ApplicationDbContext dbContext,
    AiKeyService keyService,
    IOptions<Appsettings> appSettings
)
{
    public async Task GenerateThreadTitleAsync(int threadId, string newMessageText)
    {
        MessageThread? thread = null;
        try
        {
            thread = await dbContext.MessageThreads
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == threadId);

            if (thread == null)
            {
                Console.WriteLine($"Thread with ID {threadId} not found");
                return;
            }

            var apiKey = keyService.ResolveKey(thread.UserId, appSettings.Value.ThreadTitleApiKey);

            ChatClient client = new(model: appSettings.Value.ThreadTitleModel, new ApiKeyCredential(apiKey),
                new OpenAIClientOptions()
                {
                    Endpoint = new Uri(appSettings.Value.ThreadTitleUrl)
                });

            var message = appSettings.Value.ThreadTitleGenerationPrompt
                .Replace("{MessageContent}",
                    newMessageText.Substring(0, Math.Min(appSettings.Value.MaxTitleLength, newMessageText.Length)));

            var chatMessage = new SystemChatMessage(ChatMessageContentPart.CreateTextPart(message));

            var response = await client.CompleteChatAsync(new ChatMessage[]
            {
                chatMessage
            }, new ChatCompletionOptions()
            {
                MaxOutputTokenCount = appSettings.Value.MaxTitleLength
            });

            if (response.Value.Content.Count == 0)
            {
                Console.WriteLine($"No response received for thread {threadId}");
                throw new InvalidOperationException("No response received from AI model.");
            }

            var title = response.Value.Content[0].Text.Trim();
            thread.User.ThreadVersion++;
            thread.Version = thread.User.ThreadVersion;
            thread.Title = title;

            await dbContext.SaveChangesAsync();
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

            throw;
        }
    }
}
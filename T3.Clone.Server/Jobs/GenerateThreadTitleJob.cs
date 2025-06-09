using Microsoft.EntityFrameworkCore;
using T3.Clone.Server.Data;

namespace T3.Clone.Server.Jobs;

public class GenerateThreadTitleJob(
    ApplicationDbContext dbContext
)
{
    public async Task GenerateThreadTitleAsync(int threadId, string newMessageText)
    {
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
        await Task.Delay(2500);
        thread.Title = $"Generated Title: {newMessageText.Substring(0, Math.Min(newMessageText.Length, 50))}";
        await dbContext.SaveChangesAsync();
    }
}
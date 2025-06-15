using LLMLab.Client.Caches;

namespace LLMLab.Client.Models;

public class MessageTreeDto
{
    public ThreadCache Thread { get; set; } = new ThreadCache();
    public Dictionary<int, MessageCache> Messages { get; set; } = new ();
    
    public int StartMessageId { get; set; } = 0;
    
    public Dictionary<int, MessageCache> PreviousMessages { get; set; } = new ();
    public Dictionary<int, MessageCache> NextMessages { get; set; } = new ();

    public List<MessageCache> GetMessagesForBranch(int currentBranchPath)
    {
        var messages = new List<MessageCache>();
        if (!Messages.TryGetValue(currentBranchPath, out var message))
        {
            // If the current branch path is not found, return an empty list
            return messages;
        }
        
        while(message != null)
        {
            messages.Add(message);
            message = PreviousMessages.GetValueOrDefault(message.Message.Id);
        }
        
        return messages;
    }
}
using T3.Clone.Client.Caches;

namespace T3.Clone.Dtos.Messages;

public class MessageTreeDto
{
    public ThreadCache Thread { get; set; } = new ThreadCache();
    public Dictionary<int, MessageCache> Messages { get; set; } = new ();
    
    public int StartMessageId { get; set; } = 0;
    
    public Dictionary<int, MessageCache> PreviousMessages { get; set; } = new ();
    public Dictionary<int, MessageCache> NextMessages { get; set; } = new ();
}
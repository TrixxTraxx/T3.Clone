namespace T3.Clone.Server.Data;

public class MessageThread
{
    public int Id { get; set; }
    public string Title { get; set; }
    
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    
    public ICollection<Message> Messages { get; set; } = new List<Message>();
    
    public int Version { get; set; } = 1;
    
    // The date and time when the thread was created
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // The date and time when the thread was last updated
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
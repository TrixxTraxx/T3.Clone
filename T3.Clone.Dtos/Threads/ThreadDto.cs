namespace T3.Clone.Dtos.Threads;

public class ThreadDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    
    public List<int> MessageIds { get; set; }

    // The date and time when the thread was created
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // The date and time when the thread was last updated
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    
    public int Version { get; set; } = 1;
}
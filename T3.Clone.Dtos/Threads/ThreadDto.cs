using MessagePack;

namespace T3.Clone.Dtos.Threads;

[MessagePackObject]
public class ThreadDto
{
    [Key(0)]
    public int Id { get; set; }
    [Key(1)]
    public string Title { get; set; }
    
    [Key(2)]
    public List<int> MessageIds { get; set; }
    
    [Key(3)]
    public int LastMessageId { get; set; } = 0;

    // The date and time when the thread was created
    [Key(4)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // The date and time when the thread was last updated
    [Key(5)]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [Key(6)]
    public int Version { get; set; } = 1;
    
    [Key(7)]
    public bool Deleted { get; set; }
}
namespace T3.Clone.Server.Data;

public class MessageAttachment
{
    public int Id { get; set; }
    
    public string UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;

    public AttachementContent Content { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int? MessageId { get; set; }
    public Message Message { get; set; } = null!;
}
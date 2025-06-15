namespace LLMLab.Dtos.Messages;

public class AttachmentDto
{
    public int Id { get; set; }
    
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int? MessageId { get; set; }
}
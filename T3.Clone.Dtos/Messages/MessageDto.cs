namespace T3.Clone.Dtos.Messages;

public class MessageDto
{
    public int Id { get; set; } = 0;

    public List<int> AttachmentIds { get; set; } = new List<int>();
    public string Text { get; set; } = string.Empty;

    public string ModelResponse { get; set; } = string.Empty;


    public bool Complete { get; set; } = false;
    
    public int ModelId { get; set; } = 0;
    
    public int? ThreadId { get; set; } = null;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
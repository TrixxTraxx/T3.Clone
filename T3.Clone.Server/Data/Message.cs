using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace T3.Clone.Server.Data;

public class Message
{
    public int Id { get; set; }
    
    public string Text { get; set; } = string.Empty;
    
    public string ModelResponse { get; set; } = string.Empty;
    
    public List<MessageAttachment> Attachments { get; set; }
    
    public bool Complete { get; set; } = false;

    [Required]
    public int ThreadId { get; set; }
    public MessageThread Thread { get; set; }
    
    public int ModelId { get; set; }
    public AiModel Model { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
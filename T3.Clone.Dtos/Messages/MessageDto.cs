using MessagePack;

namespace T3.Clone.Dtos.Messages;

[MessagePackObject]
public class MessageDto
{
    [Key(0)]
    public int Id { get; set; } = 0;

    [Key(1)]
    public List<int> AttachmentIds { get; set; } = new List<int>();
    
    [Key(2)]
    public string Text { get; set; } = string.Empty;

    [Key(3)]
    public string ModelResponse { get; set; } = string.Empty;

    [Key(4)]
    public int PreviousMessageId { get; set; } = 0;

    [Key(5)]
    public bool Complete { get; set; } = false;
    
    [Key(6)]
    public int ModelId { get; set; } = 0;
    
    [Key(7)]
    public int? ThreadId { get; set; } = null;

    [Key(8)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    
    [Key(9)]
    public string ThinkingResponse { get; set; } = string.Empty;
    
    [Key(10)]
    public ReasoningEffortLevel ReasoningEffortLevel { get; set; } = ReasoningEffortLevel.None;
    
    [Key(11)]
    public bool Error { get; set; } = false;
    
    [Key(12)]
    public string ErrorMessage { get; set; } = string.Empty;
}
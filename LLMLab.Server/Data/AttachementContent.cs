namespace LLMLab.Server.Data;

public class AttachementContent
{
    public int Id { get; set; }
    
    public byte[] Data { get; set; } = Array.Empty<byte>();
    
    public int AttachmentId { get; set; }
    public MessageAttachment Attachment { get; set; }
}
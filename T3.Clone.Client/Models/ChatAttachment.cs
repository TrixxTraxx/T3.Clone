using Microsoft.AspNetCore.Components.Forms;

namespace T3.Clone.Client.Models;

public class ChatAttachment
{
    public string Name { get; set; } = "";
    public long Size { get; set; }
    public string? ContentType { get; set; }
    public byte[]? Content { get; set; }
    public IBrowserFile? File { get; set; }
} 
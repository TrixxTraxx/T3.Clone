using LLMLab.Dtos.Messages;
using MessagePack;

namespace LLMLab.Client.Caches;

[MessagePackObject]
public class MessageCache
{
    [Key(0)]
    public MessageDto Message { get; set; } = new MessageDto();
    
    [Key(1)]
    public DateTime LastUpdated { get; set; }
    
    [IgnoreMember]
    public int RenderKey { get; set; } = 0;
    
    [IgnoreMember]
    public Action OnUpdated { get; set; } = () => { };
    
    [IgnoreMember]
    public Action<string> OnReasoningGenerate { get; set; } = (output) => { };
    
    [IgnoreMember]
    public Action<string> OnGenerate { get; set; } = (output) => { };
}
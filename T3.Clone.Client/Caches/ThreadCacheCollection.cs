using MessagePack;

namespace T3.Clone.Client.Caches;

[MessagePackObject]
public class ThreadCacheCollection
{
    [Key(1)]
    public List<int> ThreadIds { get; set; } = new List<int>();
    
    [Key(2)]
    public int ClientVersion { get; set; } = 0;
}
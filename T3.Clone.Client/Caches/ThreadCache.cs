using MessagePack;
using T3.Clone.Dtos.Threads;

namespace T3.Clone.Client.Caches;

[MessagePackObject]
public class ThreadCache
{
    [Key(1)]
    public ThreadDto Thread { get; set; } = new ThreadDto();
    
    [Key(2)]
    public DateTime LastUpdated { get; set; }
    
    [IgnoreMember]
    public Action OnUpdated { get; set; } = () => { };
}
using Riok.Mapperly.Abstractions;
using T3.Clone.Dtos.Threads;
using T3.Clone.Server.Data;

namespace T3.Clone.Server.Mappers;

[Mapper]
public partial class ThreadMapper
{
    public static partial ThreadDto Map(MessageThread thread);
}
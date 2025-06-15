using LLMLab.Dtos.Threads;
using LLMLab.Server.Data;
using Riok.Mapperly.Abstractions;

namespace LLMLab.Server.Mappers;

[Mapper]
public partial class ThreadMapper
{
    public static partial ThreadDto Map(MessageThread thread);
}
using Riok.Mapperly.Abstractions;
using T3.Clone.Dtos.Messages;
using T3.Clone.Server.Data;

namespace T3.Clone.Server.Mappers;

[Mapper]
public partial class MessageMapper
{
    public static partial Message Map(MessageDto message);

    public static partial MessageDto Map(Message message);
}
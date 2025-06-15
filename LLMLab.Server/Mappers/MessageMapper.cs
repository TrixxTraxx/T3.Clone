using LLMLab.Dtos.Messages;
using LLMLab.Server.Data;
using Riok.Mapperly.Abstractions;

namespace LLMLab.Server.Mappers;

[Mapper]
public partial class MessageMapper
{
    public static partial Message Map(MessageDto message);

    public static partial MessageDto Map(Message message);
}
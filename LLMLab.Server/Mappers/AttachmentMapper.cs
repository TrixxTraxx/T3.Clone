using LLMLab.Dtos.Messages;
using LLMLab.Server.Data;
using Riok.Mapperly.Abstractions;

namespace LLMLab.Server.Mappers;

[Mapper]
public partial class AttachmentMapper
{
    public static partial AttachmentDto Map(MessageAttachment attachment);
}
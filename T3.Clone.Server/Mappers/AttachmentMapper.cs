using Riok.Mapperly.Abstractions;
using T3.Clone.Dtos.Messages;
using T3.Clone.Server.Data;

namespace T3.Clone.Server.Mappers;

[Mapper]
public partial class AttachmentMapper
{
    public static partial AttachmentDto Map(MessageAttachment attachment);
}
using Riok.Mapperly.Abstractions;
using T3.Clone.Dtos.Model;
using T3.Clone.Server.Data;

namespace T3.Clone.Server.Mappers;

[Mapper]
public partial class AiModelMapper
{
    public static partial AiModelDto Map(AiModel aiModel);
}
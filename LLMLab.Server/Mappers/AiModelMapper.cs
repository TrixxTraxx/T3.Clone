using LLMLab.Dtos.Model;
using LLMLab.Server.Data;
using Riok.Mapperly.Abstractions;

namespace LLMLab.Server.Mappers;

[Mapper]
public partial class AiModelMapper
{
    public static partial AiModelDto Map(AiModel aiModel);
}
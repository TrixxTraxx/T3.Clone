using T3.Clone.Dtos.Model;
using T3.Clone.Server.Data;
using T3.Clone.Server.Mappers;

namespace T3.Clone.Server.Service;

public class AiModelService(
    ApplicationDbContext context
)
{
    public List<AiModelDto> GetModels()
    {
        var models = context.AiModels.ToList(); 
        return models.Select(x => AiModelMapper.Map(x)).ToList();
    }
}
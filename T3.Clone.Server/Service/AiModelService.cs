using System.Security.Claims;
using T3.Clone.Dtos.Model;
using T3.Clone.Server.Data;
using T3.Clone.Server.Mappers;

namespace T3.Clone.Server.Service;

public class AiModelService(
    ApplicationDbContext context,
    IHttpContextAccessor httpContextAccessor
)
{
    public List<AiModelDto> GetModels()
    {
        var userId = httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var models = context.AiModels.ToList();

        var mappedModels = models.Select(x => 
            AiModelMapper.Map(x)
        ).ToList();
        if (userId != null)
        {
            var apiKeys = context.AiModelKeys
                .Where(k => k.UserId == userId && !string.IsNullOrEmpty(k.ApiKey))
                .Select(k => k.Identifier)
                .ToList();
            // Filter models based on user-specific criteria if needed
            foreach (var model in mappedModels.Where(x => !string.IsNullOrEmpty(x.RequiredApiKey)))
            {
                model.HasRequiredApiKey = apiKeys.Contains(model.RequiredApiKey);
            }
        }
        else
        {
            // If no user is authenticated, set all models to not have the required API key
            foreach (var model in mappedModels.Where(x => !string.IsNullOrEmpty(x.RequiredApiKey)))
            {
                model.HasRequiredApiKey = false;
            }
        }
        
        return mappedModels;
    }
}
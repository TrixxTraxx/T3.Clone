using System.Security.Claims;
using T3.Clone.Server.Data;

namespace T3.Clone.Server.Service;

public class AiKeyService(
    IHttpContextAccessor httpContextAccessor,
    ApplicationDbContext dbContext
)
{
    public List<string> GetKeys()
    {
        var userId = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        return dbContext.AiModelKeys
            .Where(key => key.UserId == userId)
            .Select(key => key.Identifier)
            .ToList();
    }

    public void AddKey(string identifier, string key)
    {
        var userId = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var existingKey = dbContext.AiModelKeys
            .FirstOrDefault(k => k.UserId == userId && k.Identifier == identifier);
        
        if (existingKey != null)
        {
            existingKey.ApiKey = key;
        }
        else
        {
            dbContext.AiModelKeys.Add(new AiModelKeys
            {
                UserId = userId,
                Identifier = identifier,
                ApiKey = key
            });
        }

        dbContext.SaveChanges();
    }
}
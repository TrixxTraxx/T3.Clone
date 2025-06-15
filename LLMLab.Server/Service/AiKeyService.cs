using System.Security.Claims;
using LLMLab.Server.Data;

namespace LLMLab.Server.Service;

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
            .Where(key => key.UserId == userId && !string.IsNullOrEmpty(key.ApiKey))
            .Select(key => key.Identifier)
            .ToList();
    }

    //The Api Keys can be configured like this:
    // {OpenRouter}|{OpenAi}|HardcodedApiKey
    //it should resolve the first valid key it finds, valid keys are all keys that are not null or empty
    // The Key Configuration will have Identifiers OpenRouter and OpenAi, the Hardcoded key will not be in the Key Configuration
    public string ResolveKey(string userId, string key)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var keys = dbContext.AiModelKeys
            .Where(k => k.UserId == userId && !string.IsNullOrEmpty(k.ApiKey))
            .ToList();

        if (string.IsNullOrEmpty(key))
        {
            return keys.FirstOrDefault()?.ApiKey ?? string.Empty;
        }

        var keyParts = key.Split('|');
        foreach (var part in keyParts)
        {
            var trimmedKey = part.Trim().TrimStart('{').TrimEnd('}');
            var foundKey = keys.FirstOrDefault(k => k.Identifier.Equals(trimmedKey, StringComparison.OrdinalIgnoreCase));
            if (foundKey != null && !string.IsNullOrEmpty(foundKey.ApiKey))
            {
                return foundKey.ApiKey;
            }
            else if (!string.IsNullOrEmpty(part) && !part.Contains("{") && !part.Contains("}"))
            {
                // If the part is a hardcoded key, return it directly
                return part;
            }
        }

        return keyParts.Last(); //return last key as fallback  
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
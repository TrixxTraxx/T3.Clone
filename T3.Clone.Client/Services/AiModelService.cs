using System.Net.Http.Json;
using T3.Clone.Dtos.Model;

namespace T3.Clone.Client.Services;

public class AiModelService
{
    private static List<AiModelDto>? _models { get; set; } = null;
    
    private readonly HttpClient _httpClient;

    public AiModelService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<AiModelDto>> GetAiModels()
    {
        if (_models == null)
        {
            //fetch from  /api/aimodels
            var response = await _httpClient.GetAsync("/api/aimodels");
            
            if (response.IsSuccessStatusCode)
            {
                _models = await response.Content.ReadFromJsonAsync<List<AiModelDto>>();
            }
            else
            {
                throw new Exception("Failed to fetch AI models");
            }
        }
        return _models;
    }

    public void ClearCache()
    {
        _models = null;
    }
}
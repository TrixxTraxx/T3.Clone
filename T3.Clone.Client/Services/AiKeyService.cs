using System.Net.Http.Json;
using MudBlazor;

namespace T3.Clone.Client.Services;

public class AiKeyService
{
    private readonly HttpClient _httpClient;
    private readonly ISnackbar _snackbar;
    private readonly AiModelService _aiModelService;

    public AiKeyService(HttpClient httpClient, ISnackbar snackbar, AiModelService aiModelService)
    {
        _httpClient = httpClient;
        _snackbar = snackbar;
        _aiModelService = aiModelService;
    }

    public async Task<List<string>> GetKeysAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/AiKey");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<string>>() ?? new List<string>();
            }
            else
            {
                _snackbar.Add("Failed to fetch AI keys", Severity.Error);
                return new List<string>();
            }
        }
        catch (Exception ex)
        {
            _snackbar.Add($"Error fetching AI keys: {ex.Message}", Severity.Error);
            return new List<string>();
        }
    }

    public async Task<bool> AddKeyAsync(string identifier, string key)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/AiKey/{identifier}", key);
            
            if (response.IsSuccessStatusCode)
            {
                _aiModelService.ClearCache();
                _snackbar.Add($"{identifier} API key updated successfully", Severity.Success);
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _snackbar.Add($"Failed to update {identifier} key: {error}", Severity.Error);
                return false;
            }
        }
        catch (Exception ex)
        {
            _snackbar.Add($"Error updating {identifier} key: {ex.Message}", Severity.Error);
            return false;
        }
    }
} 
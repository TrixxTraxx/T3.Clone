using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using T3.Clone.Dtos.User;

namespace T3.Clone.Client.Services;

public class ClientAuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly NavigationManager _navigationManager;
    private readonly AppsettingsService _appSettingsService;

    public ClientAuthenticationService(HttpClient httpClient, AppsettingsService appSettingsService, NavigationManager navigationManager)
    {
        _httpClient = httpClient;
        _appSettingsService = appSettingsService;
        _navigationManager = navigationManager;
    }

    public async Task<UserDto?> GetCurrentUser()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/authentication/user");
            
            if (!response.IsSuccessStatusCode)
            {
                _navigationManager.NavigateTo(_appSettingsService.ServerUrl + "/Account/Login", true);
                return null;
            }

            var user = await response.Content.ReadFromJsonAsync<UserDto>();
            return user;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching current user (likely not logged in): {ex.Message}");
            _navigationManager.NavigateTo(_appSettingsService.ServerUrl + "/Account/Login", true);
            return null;
        }
    }
}
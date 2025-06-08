using System.Net.Http.Json;
using T3.Clone.Dtos.User;

namespace T3.Clone.Client.Services;

public class ClientAuthenticationService
{
    private readonly HttpClient _httpClient;

    public ClientAuthenticationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<UserDto?> GetCurrentUser()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/authentication/user");
            
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var user = await response.Content.ReadFromJsonAsync<UserDto>();
            return user;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching current user (likely not logged in): {ex.Message}");
            return null;
        }
    }
}
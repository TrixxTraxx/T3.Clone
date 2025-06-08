using System.Net.Http.Json;
using T3.Clone.Client.Extensions;

namespace T3.Clone.Client.Services;

public class AppsettingsService
{
    public string ServerUrl { get; set; } = string.Empty;
    public string BaseAddress { get; set; } = string.Empty;
    public bool IsLoading { get; private set; } = true;

    public AppsettingsService()
    {
    }

    public async Task LoadAppSettings(string s, string configUrl)
    {
        BaseAddress = s;
        if (!IsLoading) return;
        var http = new HttpClient();
        Console.WriteLine("Using Server URL: " + configUrl);
        var response = await http.GetFromJsonAsync<AppSettings>(configUrl);
        if (response != null)
        {
            ServerUrl = response.ServerUrl;
        }
        IsLoading = false;
    }
}
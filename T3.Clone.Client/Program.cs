using CubeTimer.Client.Extensions;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using T3.Clone.Client;
using T3.Clone.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton<AppsettingsService>();
builder.Services.AddScoped(sp =>
{
    var settings = sp.GetRequiredService<AppsettingsService>();
    Console.WriteLine("Using Server URL: " + settings.ServerUrl);
    var http = new HttpClient(new CookieHandler())
    {
        BaseAddress = new Uri(settings.ServerUrl)
    };
    return http;
});

// Load the appsettings
var app = builder.Build();
var appsettingsService = app.Services.GetRequiredService<AppsettingsService>();
var configUrl = $"{builder.HostEnvironment.BaseAddress}appsettings.json";
await appsettingsService.LoadAppSettings(builder.HostEnvironment.BaseAddress, configUrl);

await app.RunAsync();

using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using T3.Clone.Client;
using T3.Clone.Client.Extensions;
using T3.Clone.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Add MudBlazor services
builder.Services.AddMudServices();
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped<ThemeService>();
builder.Services.AddScoped<SettingsService>();
builder.Services.AddScoped<StorageService>();
builder.Services.AddScoped<ClientAuthenticationService>();
builder.Services.AddSingleton<AppsettingsService>();
builder.Services.AddScoped<ThreadSyncService>();
builder.Services.AddScoped<MessageSyncService>();
builder.Services.AddScoped<AiModelService>();
builder.Services.AddScoped<AttachmentService>();
builder.Services.AddTransient<GenerationService>();

// Add GenerationService factory for MessageSyncService
builder.Services.AddScoped<Func<GenerationService>>(provider => () => provider.GetRequiredService<GenerationService>());

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

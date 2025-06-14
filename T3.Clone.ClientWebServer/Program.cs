// MinimalFileServer/Program.cs
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseStaticFiles(new StaticFileOptions()
{
    ServeUnknownFileTypes = true
});

// Crucial for Blazor WASM routing (SPA fallback)
app.MapFallbackToFile("index.html");

app.Run();
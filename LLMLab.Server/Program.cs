using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using MudBlazor.Services;
using Blazored.LocalStorage;
using LLMLab.Server.Components;
using LLMLab.Server.Components.Account;
using LLMLab.Server.Configuration;
using LLMLab.Server.Data;
using LLMLab.Server.Seeder;
using LLMLab.Server.Service;
using LLMLab.Server.Service.Models;
using LLMLab.Server.SignalR;
using LLMLab.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.Configure<Appsettings>(
    builder.Configuration.GetSection("Appsettings")
);

builder.Services.AddCors(x =>
{
    x.AddPolicy("AllowFrontend",
        corsBuilder =>
        {
            var appsettings = builder.Configuration.GetSection("Appsettings").Get<Appsettings>();
            corsBuilder.WithOrigins(appsettings.AllowedOrigins)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
});

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddOpenApi();
builder.Services.AddSignalR();

builder.Services.AddScoped<Seeder>();
builder.Services.AddScoped<ThreadService>();
builder.Services.AddScoped<MessageService>();
builder.Services.AddScoped<AttachmentService>();
builder.Services.AddScoped<AiModelService>();
builder.Services.AddScoped<AiKeyService>();
builder.Services.AddScoped<AiGenerationService>();

builder.Services.AddScoped<ChatModelProvider>();
builder.Services.AddScoped<OpenAiChat>();
builder.Services.AddScoped<OpenAiReasoningChat>();
builder.Services.AddScoped<AnthropicChat>();
builder.Services.AddScoped<GoogleChat>();

builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

// Add MudBlazor services
builder.Services.AddMudServices();

// Add ServerSettingsService for theme management  
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<ServerSettingsService>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        options.DefaultChallengeScheme = "Google";
        options.DefaultChallengeScheme = IdentityConstants.ExternalScheme;
        options.DefaultSignOutScheme = IdentityConstants.ApplicationScheme;
    })
    .AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        
        // Request additional scopes for profile information including picture
        googleOptions.Scope.Add("profile");
        googleOptions.Scope.Add("email");
        googleOptions.Scope.Add("openid");
        
        googleOptions.ClaimActions.MapJsonKey("picture", "picture", "url");
    })
    .AddIdentityCookies(options =>
    {
        //set cookie expiration to 365 days
        options.ApplicationCookie.Configure(cookie =>
        {
            // --- Cookie Settings ---
            // Set a long expiration time span. Example: 1 year.
            cookie.ExpireTimeSpan = TimeSpan.FromDays(365); 

            // If the user is active, the cookie expiration time will be renewed.
            cookie.SlidingExpiration = true; 

            // --- Domain Settings ---
            // Set the domain for the cookie. 
            var appSettings = builder.Configuration.GetSection("Appsettings").Get<Appsettings>();
            cookie.Cookie.Domain = appSettings.CookieDomain; 

            // --- Security Settings ---
            // The cookie is essential for authentication, make it HttpOnly.
            cookie.Cookie.HttpOnly = true; 
            // Ensure the cookie is only sent over HTTPS.
            cookie.Cookie.SecurePolicy = CookieSecurePolicy.Always; 
            cookie.Cookie.SameSite = SameSiteMode.Lax; 

            // Optional: Customize the cookie name
            cookie.Cookie.Name = "T3.Clone.AuthCookie";
        });
    });

// var connectionString = "";
// builder.AddSqlServerDbContext<ApplicationDbContext>(connectionName: "t3CloneDb", settings =>
// {
//     connectionString = settings.ConnectionString;
// });
// var connectionString = builder.Configuration.GetConnectionString("t3CloneDb") ??
//                        throw new InvalidOperationException("Connection string 't3CloneDb' not found.");
//
// builder.Services.AddDbContext<ApplicationDbContext>(options =>
//     options.UseSqlServer(connectionString + ";MultipleActiveResultSets=true;"));

var connectionString = builder.Configuration.GetConnectionString("t3CloneDb") ??
                       throw new InvalidOperationException("Connection string 't3CloneDb' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString + ";MultipleActiveResultSets=true;"));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.SignIn.RequireConfirmedEmail = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

Console.WriteLine("Using connection string: " + connectionString);
builder.Services.AddHangfire(configuration => configuration
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.FromSeconds(15),
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    })
);


builder.Services.AddHangfireServer();

var mailgunApiKey = builder.Configuration["Mailgun:ApiKey"];
var mailgunDomain = builder.Configuration["Mailgun:Domain"];
var fromAddress = builder.Configuration["Mailgun:From"];

builder.Services.AddSingleton<IEmailSender<ApplicationUser>>(
    new MailgunEmailSender(mailgunApiKey, mailgunDomain, fromAddress));

var app = builder.Build();

// auto apply migrations
_ = Task.Run(async () =>
{
    await Task.Delay(TimeSpan.FromSeconds(2));
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.MigrateAsync();
            var seeder = scope.ServiceProvider.GetService<Seeder>();
            seeder!.Seed();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during database migration:");
            Console.WriteLine(ex);
            throw;
        }
    }
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    
    app.MapOpenApi();

    app.MapScalarApiReference();
}
else
{
    app.UseForwardedHeaders();
    
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Configure Hangfire dashboard for admins only
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.MapStaticAssets();

app.UseStaticFiles(new StaticFileOptions()
{
    ServeUnknownFileTypes = true
});

app.UseAntiforgery();

app.UseAuthorization();


app.MapControllers();

app.MapHub<MessageHub>("/messageHub");
app.MapHub<ThreadHub>("/threadHub");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();

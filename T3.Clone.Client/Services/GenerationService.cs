using Microsoft.AspNetCore.SignalR.Client;
using T3.Clone.Client.Caches;
using T3.Clone.Client.Services;

namespace T3.Clone.Client.Services;

/// <summary>
/// Service for connecting to the SignalR MessageHub and handling AI generation operations.
/// 
/// Usage example:
/// ```csharp
/// // Inject the service
/// @inject GenerationService GenerationService
/// 
/// // Connect to a specific message
/// await GenerationService.ConnectAsync(messageId);
/// 
/// // Subscribe to events
/// GenerationService.TokenReceived += (token) => {
///     // Handle received token
///     Console.WriteLine($"Received token: {token}");
/// };
/// 
/// GenerationService.GenerationStopped += (messageId) => {
///     // Handle generation stopped
///     Console.WriteLine($"Generation stopped for message: {messageId}");
/// };
/// 
/// // Stop generation
/// await GenerationService.StopGeneration();
/// 
/// // Send a token (if implementing client-side generation)
/// await GenerationService.OnTokenGenerated("Hello");
/// 
/// // Disconnect when done
/// await GenerationService.DisconnectAsync();
/// ```
/// </summary>
public class GenerationService : IAsyncDisposable
{
    private readonly AppsettingsService _appsettingsService;
    private HubConnection? _hubConnection;
    private int? _currentMessageId;

    public event Action<string>? TokenReceived;
    public event Action<int>? GenerationStopped;
    public event Action<object>? MessageReceived;
    public event Action<Exception>? ConnectionError;
    public event Action? Connected;
    public event Action? Disconnected;

    public GenerationService(AppsettingsService appsettingsService)
    {
        _appsettingsService = appsettingsService;
    }

    public async Task ConnectAsync(MessageCache cache)
    {
        if (_hubConnection != null)
        {
            await DisconnectAsync();
        }

        _currentMessageId = messageId;

        var hubUrl = $"{_appsettingsService.ServerUrl.TrimEnd('/')}/MessageHub?messageId={messageId}";
        
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        // Set up event handlers
        _hubConnection.On<string>("ReceiveNewToken", (token) =>
        {
            TokenReceived?.Invoke(token);
        });

        _hubConnection.On<int>("GenerationStopped", (messageId) =>
        {
            GenerationStopped?.Invoke(messageId);
        });

        _hubConnection.On<object>("NewMessage", (message) =>
        {
            MessageReceived?.Invoke(message);
        });

        // Connection state events
        _hubConnection.Closed += (exception) =>
        {
            Disconnected?.Invoke();
            if (exception != null)
            {
                ConnectionError?.Invoke(exception);
            }
            return Task.CompletedTask;
        };

        _hubConnection.Reconnected += (connectionId) =>
        {
            Connected?.Invoke();
            return Task.CompletedTask;
        };

        _hubConnection.Reconnecting += (exception) =>
        {
            if (exception != null)
            {
                ConnectionError?.Invoke(exception);
            }
            return Task.CompletedTask;
        };

        try
        {
            await _hubConnection.StartAsync();
            Connected?.Invoke();
        }
        catch (Exception ex)
        {
            ConnectionError?.Invoke(ex);
            throw;
        }
    }

    public async Task DisconnectAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
            _hubConnection = null;
        }
        _currentMessageId = null;
    }

    public async Task StopGeneration()
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            try
            {
                await _hubConnection.SendAsync("StopGeneration");
            }
            catch (Exception ex)
            {
                ConnectionError?.Invoke(ex);
                throw;
            }
        }
        else
        {
            throw new InvalidOperationException("SignalR connection is not established.");
        }
    }

    public async Task OnTokenGenerated(string token)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            try
            {
                await _hubConnection.SendAsync("OnTokenGenerated", token);
            }
            catch (Exception ex)
            {
                ConnectionError?.Invoke(ex);
                throw;
            }
        }
        else
        {
            throw new InvalidOperationException("SignalR connection is not established.");
        }
    }

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public HubConnectionState? ConnectionState => _hubConnection?.State;

    public int? CurrentMessageId => _currentMessageId;

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
    }
} 
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
/// // Connect to a specific message cache
/// await GenerationService.ConnectAsync(messageCache);
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
    private MessageCache? _currentMessageCache;

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

        _currentMessageCache = cache;

        var hubUrl = $"{_appsettingsService.ServerUrl.TrimEnd('/')}/MessageHub?messageId={cache.Message.Id}";
        
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .Build();

        // Set up event handlers
        _hubConnection.On<string>("ReceiveNewToken", (token) =>
        {
            // Update the message cache with the new token
            if (_currentMessageCache != null)
            {
                _currentMessageCache.Message.ModelResponse += token;
                _currentMessageCache.LastUpdated = DateTime.UtcNow;
                
                // Emit events to the cache and external subscribers
                _currentMessageCache.OnGenerate?.Invoke(token);
                _currentMessageCache.OnUpdated?.Invoke();
            }
            
            TokenReceived?.Invoke(token);
        });

        _hubConnection.On<int>("GenerationStopped", (messageId) =>
        {
            // Mark generation as complete
            if (_currentMessageCache != null && _currentMessageCache.Message.Id == messageId)
            {
                _currentMessageCache.Message.Complete = true;
                _currentMessageCache.LastUpdated = DateTime.UtcNow;
                _currentMessageCache.OnUpdated?.Invoke();
            }
            
            GenerationStopped?.Invoke(messageId);
        });

        _hubConnection.On<object>("NewMessage", (message) =>
        {
            // Handle new message events
            if (_currentMessageCache != null)
            {
                _currentMessageCache.LastUpdated = DateTime.UtcNow;
                _currentMessageCache.OnUpdated?.Invoke();
            }
            
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
        _currentMessageCache = null;
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

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public HubConnectionState? ConnectionState => _hubConnection?.State;

    public MessageCache? CurrentMessageCache => _currentMessageCache;

    public int? CurrentMessageId => _currentMessageCache?.Message.Id;

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
    }

    /// <summary>
    /// Creates a generation session for a message cache and automatically handles connection management.
    /// This method sets up event handlers on the MessageCache and connects to the SignalR hub.
    /// </summary>
    /// <param name="cache">The MessageCache to connect to</param>
    /// <param name="onStateChanged">Optional callback when the UI should update</param>
    /// <returns>A task that completes when the connection is established</returns>
    public async Task StartGenerationSession(MessageCache cache, Action? onStateChanged = null)
    {
        await ConnectAsync(cache);
        
        // Set up additional event handlers if a state change callback is provided
        if (onStateChanged != null)
        {
            TokenReceived += (_) => onStateChanged();
            GenerationStopped += (_) => onStateChanged();
            Connected += onStateChanged;
            Disconnected += onStateChanged;
        }
    }

    /// <summary>
    /// Reconnects to a different MessageCache if needed.
    /// </summary>
    /// <param name="newCache">The new MessageCache to connect to</param>
    /// <returns>A task that completes when the connection is reestablished</returns>
    public async Task SwitchMessageCache(MessageCache newCache)
    {
        if (_currentMessageCache?.Message.Id != newCache.Message.Id)
        {
            await ConnectAsync(newCache);
        }
    }

    /// <summary>
    /// Clears all events and disposes the connection properly.
    /// Use this when you want to completely reset the service state.
    /// </summary>
    public async Task ResetAsync()
    {
        // Clear all event handlers
        TokenReceived = null;
        GenerationStopped = null;
        MessageReceived = null;
        ConnectionError = null;
        Connected = null;
        Disconnected = null;
        
        await DisconnectAsync();
    }
} 
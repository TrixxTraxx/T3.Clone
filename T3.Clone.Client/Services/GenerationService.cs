using Microsoft.AspNetCore.SignalR.Client;
using T3.Clone.Client.Caches;
using T3.Clone.Dtos.Messages;

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

        var hubUrl = $"{_appsettingsService.ServerUrl}/MessageHub?messageId={cache.Message.Id}";
        
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .Build();

        // Set up event handlers
        _hubConnection.On<string, bool>("ReceiveNewToken", (token, isThinking) =>
        {
            Console.WriteLine($"Received new token: {token}");

            if (isThinking)
            {
                _currentMessageCache.Message.ThinkingResponse += token;
            }
            else
            {
                _currentMessageCache.Message.ModelResponse += token;
            }
            _currentMessageCache.LastUpdated = DateTime.UtcNow;
            
            // Emit events to the cache and external subscribers
            _currentMessageCache.OnGenerate?.Invoke(token);
        });

        _hubConnection.On<MessageDto>("GenerationStopped", (message) =>
        {
            Console.WriteLine($"Received GenerationStopped for messageId: {message.Id}");
            
            _currentMessageCache.Message = message;
            _currentMessageCache.LastUpdated = DateTime.UtcNow;
            _currentMessageCache.OnUpdated?.Invoke();
            
            // disconnect from the hub
            _hubConnection?.StopAsync().ContinueWith(t => 
            {
                if (t.IsFaulted)
                {
                    Console.WriteLine($"Error stopping hub connection: {t.Exception?.Message}");
                }
            });
        });

        _hubConnection.On<MessageDto>("NewMessage", (message) =>
        {
            Console.WriteLine($"Received new message: {message}");
            
            // Handle new message events
            _currentMessageCache.Message = message;
            _currentMessageCache.LastUpdated = DateTime.UtcNow;
            _currentMessageCache.OnUpdated?.Invoke();
        });

        try
        {
            await _hubConnection.StartAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to connect to Hub: {ex.Message}");
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
                Console.WriteLine(ex);
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
    public async Task StartGenerationSession(MessageCache cache)
    {
        await ConnectAsync(cache);
    }

    /// <summary>
    /// Clears all events and disposes the connection properly.
    /// Use this when you want to completely reset the service state.
    /// </summary>
    public async Task ResetAsync()
    {
        await DisconnectAsync();
    }
} 
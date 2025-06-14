using System.Text.Json;
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
    private MessageSyncService _messageService;
    
    private string tokenCache = string.Empty;

    public GenerationService(AppsettingsService appsettingsService, MessageSyncService messageService)
    {
        _appsettingsService = appsettingsService;
        _messageService = messageService;
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
            if (isThinking)
            {
                _currentMessageCache.Message.ThinkingResponse += token;
                // Emit events to the cache and external subscribers
                _currentMessageCache.OnReasoningGenerate?.Invoke(token);
            }
            else
            {
                PassTokensBatched(token);
            }
            _currentMessageCache.LastUpdated = DateTime.UtcNow;
        });

        _hubConnection.On<MessageDto>("GenerationStopped", (message) =>
        {
            Console.WriteLine($"Message {message.Id} has been stopped. Final response: {JsonSerializer.Serialize(message)}");
            _currentMessageCache.Message = message;
            _currentMessageCache.LastUpdated = DateTime.UtcNow;
            _currentMessageCache.OnUpdated?.Invoke();
            _messageService.UpdateMessageCache(_currentMessageCache);
            
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
            Console.WriteLine($"Message {message.Id} has been updated. new message: {JsonSerializer.Serialize(message)}");
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

    private void PassTokensBatched(string token)
    {
        if (string.IsNullOrEmpty(tokenCache))
        {
            tokenCache = token;
            _ = Task.Run(async () =>
            {
                await Task.Delay(16); // 60 times per second max
                if (!string.IsNullOrEmpty(tokenCache))
                {
                    _currentMessageCache.Message.ModelResponse += tokenCache;
                    _currentMessageCache.OnGenerate?.Invoke(tokenCache);
                    tokenCache = string.Empty; // Clear the cache after sending
                }
            });
        }
        else
        {
            tokenCache += token;
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
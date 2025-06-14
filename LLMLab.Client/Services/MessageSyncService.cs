using System.Net.Http.Json;
using LLMLab.Client.Caches;
using LLMLab.Client.Models;
using LLMLab.Dtos.Messages;
using MudBlazor;

namespace LLMLab.Client.Services;

public class MessageSyncService
{
    private Dictionary<int, MessageCache> _messageCaches { get; } = new();
    private HashSet<int> _activeGenerations { get; } = new();

    private readonly HttpClient _http;
    private readonly ISnackbar _snackbar;
    private readonly StorageService _storageService;
    private readonly ThreadSyncService _threadSyncService;

    public MessageSyncService(HttpClient http, ISnackbar snackbar, StorageService storageService, ThreadSyncService threadSyncService)
    {
        _http = http;
        _snackbar = snackbar;
        _storageService = storageService;
        _threadSyncService = threadSyncService;
    }
    
    public async Task<MessageCache> GetMessageCache(int messageId)
    {
        if (_messageCaches.TryGetValue(messageId, out var messageCache))
        {
            return messageCache;
        }

        // Load from storage or create a new cache
        return await GetMessageCacheFromServerOrStorage(messageId);
    }

    private async Task<MessageCache> GetMessageCacheFromServerOrStorage(int messageId)
    {
        // Load from storage
        var storedMessageCache = await _storageService.ReadObjectAsync<MessageCache>($"MessageCache_{messageId}");
        if (storedMessageCache != null)
        {
            _messageCaches[messageId] = storedMessageCache;
            return storedMessageCache;
        }

        // Load from Server
        return await GetMessageCacheFromServer(messageId);
        
    }

    private async Task<MessageCache> GetMessageCacheFromServer(int messageId)
    {
        var response = await _http.GetAsync($"api/message/{messageId}");
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to load message cache for messageId {messageId}");
        }
        var messageDto = await response.Content.ReadFromJsonAsync<MessageDto>();
        if (messageDto == null)
        {
            throw new Exception($"Message with ID {messageId} not found.");
        }
        var messageCache = new MessageCache()
        {
            Message = messageDto,
            LastUpdated = DateTime.UtcNow
        };
        _messageCaches[messageId] = messageCache;
        
        // Store in local storage
        await _storageService.StoreObjectAsync($"MessageCache_{messageId}", messageCache);
        
        return messageCache;
    }

    public async Task<MessageTreeDto> GetMessageTree(int threadId)
    {
        var thread = await _threadSyncService.GetThreadCache(threadId);
        
        if (thread == null)
        {
            throw new Exception($"Thread with ID {threadId} not found.");
        }
        
        //get all messages for the thread
        Dictionary<int, MessageCache> caches = new();
        foreach (var messageId in thread.Thread.MessageIds)
        {
            try
            {
                // Get the message cache, this will update it if needed
                var messageCache = await GetMessageCache(messageId);
                caches.Add(messageCache.Message.Id, messageCache);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to get message cache for messageId {messageId}: {ex.Message}");
            }
        }
        
        // Create the message tree
        var messageTree = new MessageTreeDto
        {
            Thread = thread,
            Messages = caches,
            StartMessageId = caches.MaxBy(x => x.Value.Message.CreatedAt).Value.Message.Id,
            PreviousMessages = new Dictionary<int, MessageCache>(),
            NextMessages = new Dictionary<int, MessageCache>()
        };
        
        // Populate previous and next messages
        foreach (var message in caches.Values)
        {
            if (message.Message.PreviousMessageId != 0 && caches.TryGetValue(message.Message.PreviousMessageId, out var previousMessage))
            {
                messageTree.PreviousMessages[message.Message.Id] = previousMessage;
            }
            if (message.Message.Id != 0 && caches.TryGetValue(message.Message.Id, out var nextMessage))
            {
                messageTree.NextMessages[message.Message.Id] = nextMessage;
            }
        }
        
        return messageTree;
    }
    
    public async Task UpdateCacheInBackground()
    {
        try
        {
            await Task.Yield();
            // Get the thread cache to ensure it exists
            var threadCache = await _threadSyncService.GetThreads();
            
            await Task.Yield();
            // update all messages from all threads if the thread cache is not null
            foreach (var cache in threadCache)
            {
                foreach (var messageId in cache.Thread.MessageIds)
                {
                    try
                    {
                        //unblock the UI
                        await Task.Yield();
                        
                        // Getting the message cache automatically updates it
                        _ = await GetMessageCache(messageId);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _snackbar.Add($"Failed to update message cache: {ex.Message}", Severity.Error);
        }
    }

    public async Task<MessageDto> SendMessage(MessageCache cache)
    {
        var response = await _http.PostAsJsonAsync("api/message", cache.Message);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to send message: {response.ReasonPhrase}");
        }
        
        var sentMessage = await response.Content.ReadFromJsonAsync<MessageDto>();
        if (sentMessage == null)
        {
            throw new Exception("Failed to parse sent message.");
        }

        // Update the cache
        cache.Message = sentMessage;
        cache.LastUpdated = DateTime.UtcNow;
        
        _messageCaches[sentMessage.Id] = cache;
        
        // Store in local storage
        await _storageService.StoreObjectAsync($"MessageCache_{sentMessage.Id}", cache);
        
        // refresh the thread cache now
        await _threadSyncService.Update();
        
        return sentMessage;
    }

    /// <summary>
    /// Updates a message cache and persists it to storage
    /// </summary>
    public async Task UpdateMessageCache(MessageCache messageCache)
    {
        _messageCaches[messageCache.Message.Id] = messageCache;
        await _storageService.StoreObjectAsync($"MessageCache_{messageCache.Message.Id}", messageCache);
    }

    /// <summary>
    /// Gets all cached messages for cleanup or management purposes
    /// </summary>
    public Dictionary<int, MessageCache> GetAllCachedMessages()
    {
        return new Dictionary<int, MessageCache>(_messageCaches);
    }
}
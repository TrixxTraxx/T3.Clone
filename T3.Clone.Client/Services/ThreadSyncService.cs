using System.Net.Http.Json;
using MudBlazor;
using T3.Clone.Client.Caches;
using T3.Clone.Dtos.Threads;

namespace T3.Clone.Client.Services;

public class ThreadSyncService
{
    private ThreadCacheCollection? _threadCacheCollection = null;
    private List<ThreadCache> _threadCaches { get; } = new List<ThreadCache>();

    private readonly HttpClient _http;
    private readonly ISnackbar _snackbar;
    private readonly StorageService _storageService;
    
    public Action<List<ThreadCache>>? ThreadsUpdated;

    public ThreadSyncService(HttpClient http, StorageService storageService, ISnackbar snackbar)
    {
        _http = http;
        _storageService = storageService;
        _snackbar = snackbar;
    }

    private async Task StoreThreadCacheCollection(ThreadCacheCollection threadCacheCollection)
    {
        _threadCacheCollection = threadCacheCollection;
        await _storageService.StoreObjectAsync("ThreadCacheCollection", threadCacheCollection);
    }

    public async Task<List<ThreadCache>> GetThreads(Action<List<ThreadCache>>? update = null)
    {
        var threadCollection = await GetThreadCollection();
        
        List<ThreadCache> threadCaches = new List<ThreadCache>();
        foreach (var threadId in threadCollection.ThreadIds)
        {
            var threadCache = await GetThreadCache(threadId);
            if (threadCache != null)
            {
                threadCaches.Add(threadCache);
            }
        }

        if (update != null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Yield();
                    await UpdateThreadCaches(update);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    _snackbar.Add($"Error updating threads: {ex.Message}", Severity.Error);
                }
            });
        }
        
        return threadCaches;
    }

    public async Task<ThreadCache?> GetThreadCache(int threadId)
    {
        var threadCache = _threadCaches.FirstOrDefault(tc => tc.Thread.Id == threadId);
        if (threadCache == null)
        {
            // Get from storage
            threadCache = await _storageService.ReadObjectAsync<ThreadCache>($"ThreadCache_{threadId}");
            if(threadCache != null)
            {
                // Add to the local cache if not already present
                _threadCaches.Add(threadCache);
            }
        }
        return threadCache;
    }

    private async Task<ThreadCacheCollection> GetThreadCollection()
    {
        if (_threadCacheCollection == null)
        {
            _threadCacheCollection = (await _storageService.ReadObjectAsync<ThreadCacheCollection>("ThreadCacheCollection")) ?? new ThreadCacheCollection();
        }
        return _threadCacheCollection;
    }

    private async Task UpdateThreadCaches(Action<List<ThreadCache>> update)
    {
        var threadCacheCollection = await GetThreadCollection();
        
        // call /api/Thread/threads?clientVersion=threadCacheCollection.ClientVersion
        var response = await _http.GetAsync($"api/Thread/threads?clientVersion={threadCacheCollection.ClientVersion}");

        if (!response.IsSuccessStatusCode)
        {
            //something went very wrong
            _snackbar.Add("Failed to sync threads", Severity.Error);
            return;
        }
        
        var updateDto = await response.Content.ReadFromJsonAsync<ThreadUpdateDto>();
        
        Console.WriteLine($"Updating {updateDto!.UpdatedThreads.Count} threads");

        foreach (var thread in updateDto!.UpdatedThreads)
        {
            Console.WriteLine($"updating thread with Id: {thread.Id}");
            try
            {
                //find thread cache by id
                var threadCache = _threadCaches.FirstOrDefault(tc => tc.Thread.Id == thread.Id);
                if (threadCache == null)
                {
                    //create new thread cache
                    threadCache = new ThreadCache()
                    {
                        Thread = thread,
                        LastUpdated = DateTime.Now
                    };
                    threadCacheCollection.ThreadIds.Add(thread.Id);
                    _threadCaches.Add(threadCache);
                }
                else
                {
                    //update existing thread cache
                    threadCache.Thread = thread;
                    threadCache.LastUpdated = DateTime.Now;
                }
                // Store the updated thread cache
                await StoreThreadCache(threadCache);
            }
            catch (Exception ex)
            {
                _snackbar.Add($"Error updating thread {thread.Id}: {ex.Message}", Severity.Error);
                Console.WriteLine(ex);
                continue;
            }
            Console.WriteLine($"Thread {thread.Id} updated");
        }

        try
        {
            Console.WriteLine($"Updated {_threadCaches.Count} threads");
            update?.Invoke(_threadCaches);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        
        threadCacheCollection.ClientVersion = updateDto!.UpdatedVersion;
        await StoreThreadCacheCollection(threadCacheCollection);
    }

    private async Task StoreThreadCache(ThreadCache threadCache)
    {
        // Store the thread cache in the collection
        await _storageService.StoreObjectAsync($"ThreadCache_{threadCache.Thread.Id}", threadCache);
    }

    public async Task Update()
    {
        var updatedThreads = await GetThreads();
        ThreadsUpdated?.Invoke(updatedThreads);
    }
}
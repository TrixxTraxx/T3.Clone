using System.Net.Http.Json;
using System.Text.Json;
using MudBlazor;
using T3.Clone.Client.Caches;
using T3.Clone.Dtos.Threads;

namespace T3.Clone.Client.Services;

public class ThreadSyncService
{
    private ThreadCacheCollection? _threadCacheCollection = null;
    private List<ThreadCache> _threadCaches { get; set; } = new List<ThreadCache>();

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
        
        foreach (var threadId in threadCollection.ThreadIds.Distinct())
        {
            await GetThreadCache(threadId);
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
        //Console.WriteLine("Thread Caches loaded: " + _threadCaches.Count);
        //Console.WriteLine("Thread Caches updated: " + threadCollection.ThreadIds.Distinct().Count());
        //Console.WriteLine("Thread Caches: " + JsonSerializer.Serialize(_threadCaches.Select(x => x.Thread), new JsonSerializerOptions { WriteIndented = true }));
        
        //there is some bug where the first thread gets duplicated for some reason and then disappears later
        // I cant find the root cause, so we just remove duplicates here
        _threadCaches = _threadCaches.DistinctBy(x => x.Thread.Id).ToList();
        return _threadCaches;
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
                //Console.WriteLine("Adding thread cache with Id: " + threadId);
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
        
        // call /api/Threads?clientVersion=threadCacheCollection.ClientVersion
        var response = await _http.GetAsync($"api/Threads?clientVersion={threadCacheCollection.ClientVersion}");

        if (!response.IsSuccessStatusCode)
        {
            //something went very wrong
            _snackbar.Add("Failed to sync threads", Severity.Error);
            return;
        }
        
        var updateDto = await response.Content.ReadFromJsonAsync<ThreadUpdateDto>();
        
        //Console.WriteLine($"Updating {updateDto!.UpdatedThreads.Count} threads");

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
                    Console.WriteLine($"Added thread with Id: {threadCache.Thread.Id}");
                    _threadCaches.Add(threadCache);
                }
                else
                {
                    Console.WriteLine($"Updated thread with Id: {threadCache.Thread.Id}");
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

        if (updateDto!.UpdatedThreads?.Any() ?? false)
        {
            try
            {
                //Console.WriteLine($"{_threadCaches.Count} Threads are up to date!");
                update?.Invoke(_threadCaches);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
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
        _ = await GetThreads(threads =>
        {
            ThreadsUpdated?.Invoke(threads);            
        });
    }
    
    public void ClearMemoryCache()
    {
        _threadCaches.Clear();
        _threadCacheCollection = null;
    }
    
    public async Task UpdateThread(ThreadCache cache)
    {
        var response = await _http.PutAsync($"api/Threads",
            new StringContent(JsonSerializer.Serialize(cache.Thread), System.Text.Encoding.UTF8, "application/json"));
        if (response.IsSuccessStatusCode)
        {
            // Update the thread cache in memory
            await Update();
        }
        else
        {
            //update to get back the old thread
            _threadCaches.Remove(cache);
            await Update();
            _snackbar.Add("Failed to delete thread.", Severity.Error);
        }
    }
}
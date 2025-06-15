using MessagePack;
using Microsoft.JSInterop;

namespace LLMLab.Client.Services;

public class StorageService
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;
    
    public StorageService(IJSRuntime jsRuntime)
    {
        _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./js/indexedDbModule.js").AsTask());
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_moduleTask.IsValueCreated)
        {
            var module = await _moduleTask.Value;
            await module.DisposeAsync();
        }
    }
    
    public async Task StoreObjectAsync<T>(string key, T obj)
    {
        //Stopwatch sw = Stopwatch.StartNew();
        var module = await _moduleTask.Value;
        
        // Serialize using MessagePack on a background thread
        byte[] binary = MessagePackSerializer.Serialize(obj);
        await Task.Yield();
         
        // Convert to base64 string for storage
        string base64 = Convert.ToBase64String(binary);
        await Task.Yield();
        
        await module.InvokeVoidAsync("storeObject", key, "msgpack:" + base64);
        //sw.Stop();
        //Console.WriteLine($"Serialization took: {sw.ElapsedMilliseconds} ms");
    }
    
    public async Task<T> ReadObjectAsync<T>(string key)
    {
        var module = await _moduleTask.Value;
        await Task.Yield();
        string data = await module.InvokeAsync<string>("readObject", key);
        //Stopwatch sw = Stopwatch.StartNew();

        if (string.IsNullOrEmpty(data))
            return default;
        
        await Task.Yield();
        string base64 = data.Substring(8); // Remove "msgpack:" prefix
        byte[] binary = Convert.FromBase64String(base64);
        await Task.Yield();
        T obj = MessagePackSerializer.Deserialize<T>(binary);
        //sw.Stop();
        //Console.WriteLine($"Deserialization took: {sw.ElapsedMilliseconds} ms");
        return obj;
    }
    
    public async Task<List<string>> GetKeysAsync()
    {
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<List<string>>("getKeys");
    }
    
    public async Task RemoveObjectAsync(string key)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("removeObject", key);
    }
}
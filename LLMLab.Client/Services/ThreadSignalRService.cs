using System.Text.Json;
using LLMLab.Client.Caches;
using LLMLab.Dtos.Messages;
using Microsoft.AspNetCore.SignalR.Client;

namespace LLMLab.Client.Services;

public class ThreadSignalRService: IAsyncDisposable
{
    private readonly AppsettingsService _appsettingsService;
    private HubConnection? _hubConnection;
    private ThreadSyncService _threadSyncService;
    

    public ThreadSignalRService(AppsettingsService appsettingsService, ThreadSyncService threadSyncService)
    {
        _appsettingsService = appsettingsService;
        _threadSyncService = threadSyncService;
    }

    public async Task ConnectAsync()
    {
        if (_hubConnection != null)
        {
            await DisconnectAsync();
        }

        var hubUrl = $"{_appsettingsService.ServerUrl}/threadHub";
        
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .Build();

        // Set up event handlers
        _hubConnection.On("ThreadsUpdated", () =>
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await _threadSyncService.Update();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating threads: {ex.Message}");
                }
            });
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
    }

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public HubConnectionState? ConnectionState => _hubConnection?.State;

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
    }
} 
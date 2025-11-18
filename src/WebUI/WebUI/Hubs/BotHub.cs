using Microsoft.AspNetCore.SignalR;
using WebUI.Models;

namespace WebUI.Hubs;

public class BotHub : Hub
{
    private readonly ILogger<BotHub> _logger;

    public BotHub(ILogger<BotHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task RequestBotState()
    {
        // This will be called by clients to request current state
        _logger.LogInformation("Bot state requested by: {ConnectionId}", Context.ConnectionId);
    }
}

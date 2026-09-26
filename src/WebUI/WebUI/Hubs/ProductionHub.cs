using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace WebUI.Hubs;

[Authorize]
public sealed class ProductionHub : Hub
{
    public Task SubscribeToWorkspace(string workspace = "main")
        => Groups.AddToGroupAsync(Context.ConnectionId, workspace);
}

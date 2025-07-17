using Microsoft.AspNetCore.SignalR;

namespace HexMaster.BlazorChat.Server.Hubs;

public class ChatHub : Hub
{
    public async Task JoinChat(string username)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "ChatRoom");
        await Clients.Group("ChatRoom").SendAsync("UserJoined", username);
    }

    public async Task LeaveChat(string username)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "ChatRoom");
        await Clients.Group("ChatRoom").SendAsync("UserLeft", username);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "ChatRoom");
        await base.OnDisconnectedAsync(exception);
    }
}

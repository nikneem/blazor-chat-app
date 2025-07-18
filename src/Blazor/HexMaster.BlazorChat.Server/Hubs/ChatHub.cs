using Microsoft.AspNetCore.SignalR;

namespace HexMaster.BlazorChat.Server.Hubs;

public class ChatHub : Hub
{
    private static int _connectionCount = 0;

    public async Task JoinChat(string username)
    {
        Console.WriteLine($"Hub: User {username} joining chat. ConnectionId: {Context.ConnectionId}");
        await Groups.AddToGroupAsync(Context.ConnectionId, "ChatRoom");
        await Clients.Group("ChatRoom").SendAsync("UserJoined", username);
        Console.WriteLine($"Hub: User {username} successfully joined chat. Total connections: {_connectionCount}");
    }

    public async Task LeaveChat(string username)
    {
        Console.WriteLine($"Hub: User {username} leaving chat. ConnectionId: {Context.ConnectionId}");
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "ChatRoom");
        await Clients.Group("ChatRoom").SendAsync("UserLeft", username);
        Console.WriteLine($"Hub: User {username} successfully left chat");
    }

    public override async Task OnConnectedAsync()
    {
        _connectionCount++;
        Console.WriteLine($"Hub: Client connected. ConnectionId: {Context.ConnectionId}. Total connections: {_connectionCount}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _connectionCount--;
        Console.WriteLine($"Hub: Client disconnected. ConnectionId: {Context.ConnectionId}. Total connections: {_connectionCount}. Exception: {exception?.Message ?? "None"}");
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "ChatRoom");
        await base.OnDisconnectedAsync(exception);
    }
}

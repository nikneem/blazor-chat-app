using HexMaster.BlazorChat.Chat.ExtensionMethods;
using HexMaster.BlazorChat.Server.Endpoints.Chat;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(args);

        builder.AddServiceDefaults();
        builder.AddAzureTableClient("chatmessages");
        builder.AddChatMessages();

        // Add SignalR
        builder.Services.AddSignalR();

        var app = builder.Build();

        app.MapDefaultEndpoints().MapChatEndpoints();
        
        // Map SignalR hub
        app.MapHub<HexMaster.BlazorChat.Server.Hubs.ChatHub>("/chathub");

        app.Run();
    }
}
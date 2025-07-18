using HexMaster.BlazorChat.Chat.ExtensionMethods;
using HexMaster.BlazorChat.Server.Endpoints.Chat;
using HexMaster.BlazorChat.Chat;
using System.Text.Json;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(args);

        builder.AddServiceDefaults();
        builder.AddAzureTableClient("chatmessages");
        builder.AddChatMessages();

        // Add SignalR
        builder.Services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
        })
        .AddJsonProtocol(options =>
        {
            options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.PayloadSerializerOptions.WriteIndented = false;
            // Configure SignalR to use the same serialization context as the API endpoints
            options.PayloadSerializerOptions.TypeInfoResolverChain.Insert(0, ChatMessageSerializationContext.Default);
        });

        // Add CORS for SignalR
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        var app = builder.Build();

        // Use CORS
        app.UseCors();

        app.MapDefaultEndpoints().MapChatEndpoints();
        
        // Map SignalR hub
        app.MapHub<HexMaster.BlazorChat.Server.Hubs.ChatHub>("/chathub");

        app.Run();
    }
}
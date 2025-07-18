using HexMaster.BlazorChat.Chat.Abstractions.DataTransferObjects;
using HexMaster.BlazorChat.Chat.Abstractions.Services;
using HexMaster.BlazorChat.Server.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace HexMaster.BlazorChat.Server.Endpoints.Chat;

public static class ChatApplicationEndpoints
{

    public static WebApplication MapChatEndpoints(this WebApplication app)
    {
        var chatGroup = app.MapGroup("/chat")
            .WithTags("Chat");
            
        chatGroup.MapPost("/messages", async (CreateChatMessageRequest request, IChatMessagesService chatMessageService, IHubContext<ChatHub> hubContext, HttpContext context) =>
        {
            var message = await chatMessageService.CreateMessage(request, context.RequestAborted);

            Console.WriteLine($"Server: Created message {message.Id}: '{message.Message}' from {message.Sender}");
            Console.WriteLine($"Server: Broadcasting to SignalR clients...");

            try
            {
                // Check if there are any connected clients
                var connectedCount = hubContext.Clients.All.GetType().Name;
                Console.WriteLine($"Server: Hub context type: {connectedCount}");
                
                // Try a simpler message first
                Console.WriteLine($"Server: Trying simple string message first...");
                await hubContext.Clients.All.SendAsync("TestMessage", "Hello World", context.RequestAborted);
                Console.WriteLine($"Server: Simple message sent successfully");
                
                // Now try the actual message
                Console.WriteLine($"Server: Trying full message...");
                await hubContext.Clients.All.SendAsync("ReceiveMessage", 
                    message.Id.ToString(), // Convert Guid to string to avoid serialization issues
                    message.Sender, 
                    message.Message, 
                    message.CreatedOn.ToString("O"), // Convert DateTimeOffset to ISO string
                    context.RequestAborted);
                
                Console.WriteLine($"Server: SignalR broadcast completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server: SignalR broadcast failed: {ex.Message}");
                Console.WriteLine($"Server: Exception details: {ex}");
                Console.WriteLine($"Server: Stack trace: {ex.StackTrace}");
            }

            return Results.Created($"/chat/messages/{message.Id}", message);
        });
        
        chatGroup.MapGet("/messages", async (IChatMessagesService chatMessageService, HttpContext context) =>
        {
            var messages = await chatMessageService.GetMessagesAsync(context.RequestAborted);
            return Results.Ok(messages);
        });

        // Test endpoint to manually trigger SignalR message
        chatGroup.MapPost("/test-signalr", async (IHubContext<ChatHub> hubContext, HttpContext context) =>
        {
            Console.WriteLine("Server: Test SignalR endpoint called");
            try
            {
                var testId = Guid.NewGuid();
                var testSender = "System";
                var testMessage = "Test message from server";
                var testCreatedOn = DateTimeOffset.UtcNow;
                
                await hubContext.Clients.All.SendAsync("ReceiveMessage", 
                    testId, 
                    testSender, 
                    testMessage, 
                    testCreatedOn, 
                    context.RequestAborted);
                    
                Console.WriteLine("Server: Test SignalR message sent successfully");
                return Results.Ok("Test message sent");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server: Test SignalR failed: {ex.Message}");
                return Results.Problem($"Failed to send test message: {ex.Message}");
            }
        });

        return app;
    }

}

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
            
            // Broadcast the new message to all connected clients
            await hubContext.Clients.Group("ChatRoom").SendAsync("ReceiveMessage", message, context.RequestAborted);
            
            return Results.Created($"/chat/messages/{message.Id}", message);
        });
        
        chatGroup.MapGet("/messages", async (IChatMessagesService chatMessageService, HttpContext context) =>
        {
            var messages = await chatMessageService.GetMessagesAsync(context.RequestAborted);
            return Results.Ok(messages);
        });

        return app;
    }

}

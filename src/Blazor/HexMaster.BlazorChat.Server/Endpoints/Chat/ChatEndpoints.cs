using HexMaster.BlazorChat.Chat.Abstractions.DataTransferObjects;
using HexMaster.BlazorChat.Chat.Abstractions.Services;

namespace HexMaster.BlazorChat.Server.Endpoints.Chat;

public static class ChatApplicationEndpoints
{

    public static WebApplication MapChatEndpoints(this WebApplication app)
    {
        app.MapGroup("/chat")
            .WithTags("Chat")
            .MapPost("/messages", async (CreateChatMessageRequest request, IChatMessagesService chatMessageService, HttpContext context) =>
            {
                var message = await chatMessageService.CreateMessage(request, context.RequestAborted);
                return Results.Created($"/chat/messages/{message.Id}", message);
            });
            //.MapGet("/messages", async (IChatMessageService chatMessageService) =>
            //{
            //    var messages = await chatMessageService.GetMessagesAsync();
            //    return Results.Ok(messages);
            //});

        return app;
    }

}

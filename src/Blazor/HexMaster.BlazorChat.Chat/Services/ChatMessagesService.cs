using HexMaster.BlazorChat.Chat.Abstractions.DataTransferObjects;
using HexMaster.BlazorChat.Chat.Abstractions.Services;

namespace HexMaster.BlazorChat.Chat.Services;

public class ChatMessagesService : IChatMessagesService
{
    public async ValueTask<CreateChatMessageResponse> CreateMessage(CreateChatMessageRequest request, CancellationToken cancellationToken)
    {
        return await Task.FromResult(new CreateChatMessageResponse(Guid.NewGuid(),
            request.Sender,
            request.Message,
            DateTimeOffset.UtcNow
        ));
    }
}

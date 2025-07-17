using HexMaster.BlazorChat.Chat.Abstractions.DataTransferObjects;

namespace HexMaster.BlazorChat.Chat.Abstractions.Repositories;

public interface IChatMessagesRepository
{
    ValueTask<CreateChatMessageResponse> CreateMessageAsync(CreateChatMessageRequest request, CancellationToken cancellationToken);
}

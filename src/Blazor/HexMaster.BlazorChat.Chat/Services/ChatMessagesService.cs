using HexMaster.BlazorChat.Chat.Abstractions.DataTransferObjects;
using HexMaster.BlazorChat.Chat.Abstractions.Repositories;
using HexMaster.BlazorChat.Chat.Abstractions.Services;

namespace HexMaster.BlazorChat.Chat.Services;

public class ChatMessagesService : IChatMessagesService
{
    private readonly IChatMessagesRepository _repository;

    public ChatMessagesService(IChatMessagesRepository repository)
    {
        _repository = repository;
    }

    public async ValueTask<CreateChatMessageResponse> CreateMessage(CreateChatMessageRequest request, CancellationToken cancellationToken)
    {
        return await _repository.CreateMessageAsync(request, cancellationToken);
    }

    public async ValueTask<IEnumerable<ChatMessageDto>> GetMessagesAsync(CancellationToken cancellationToken)
    {
        return await _repository.GetMessagesAsync(cancellationToken);
    }
}

using Azure.Data.Tables;
using HexMaster.BlazorChat.Chat.Abstractions.DataTransferObjects;
using HexMaster.BlazorChat.Chat.Abstractions.Repositories;
using HexMaster.BlazorChat.Chat.Entities;
using HexMaster.BlazorChat.Chat.Services;

namespace HexMaster.BlazorChat.Chat.Repositories;

public class ChatMessagesRepository : IChatMessagesRepository
{
    private readonly AotTableClient _tableClient;

    public ChatMessagesRepository(TableServiceClient tableServiceClient)
    {
        _tableClient = new AotTableClient(tableServiceClient, "chatmessages", 
            new System.Text.Json.JsonSerializerOptions
            {
                TypeInfoResolver = ChatMessageSerializationContext.Default
            });
    }

    public async ValueTask<CreateChatMessageResponse> CreateMessageAsync(CreateChatMessageRequest request, CancellationToken cancellationToken)
    {
        // Ensure the table exists
        await _tableClient.CreateIfNotExistsAsync(cancellationToken);

        var messageId = Guid.NewGuid();
        var createdOn = DateTimeOffset.UtcNow;
        
        var entity = new ChatMessageEntity(messageId, request.Sender, request.Message, createdOn);

        await _tableClient.AddEntityAsync(entity, cancellationToken);

        return new CreateChatMessageResponse(messageId, request.Sender, request.Message, createdOn);
    }
}

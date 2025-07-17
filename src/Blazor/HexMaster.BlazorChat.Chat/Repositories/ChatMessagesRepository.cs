using Azure.Data.Tables;
using HexMaster.BlazorChat.Chat.Abstractions.DataTransferObjects;
using HexMaster.BlazorChat.Chat.Abstractions.Repositories;
using HexMaster.BlazorChat.Chat.Entities;
using Azure;

namespace HexMaster.BlazorChat.Chat.Repositories;

public class ChatMessagesRepository : IChatMessagesRepository
{
    private readonly TableClient _tableClient;
    private bool _tableInitialized = false;
    private const string TableName = "chatmessages";

    public ChatMessagesRepository(TableServiceClient tableServiceClient)
    {
        _tableClient = tableServiceClient.GetTableClient(TableName);
    }

    public async ValueTask<CreateChatMessageResponse> CreateMessageAsync(CreateChatMessageRequest request, CancellationToken cancellationToken)
    {
        // Ensure table exists first
        await CreateIfNotExistsAsync(cancellationToken);

        var messageId = Guid.NewGuid();
        var createdOn = DateTimeOffset.UtcNow;
        
        var entity = new ChatMessageEntity(messageId, request.Sender, request.Message, createdOn);
        await _tableClient.AddEntityAsync(entity, cancellationToken);

        return new CreateChatMessageResponse(messageId, request.Sender, request.Message, createdOn);
    }

    public async ValueTask<IEnumerable<ChatMessageDto>> GetMessagesAsync(CancellationToken cancellationToken)
    {
        // Ensure table exists first
        await CreateIfNotExistsAsync(cancellationToken);

        var messages = new List<ChatMessageDto>();
        
        await foreach (var entity in _tableClient.QueryAsync<ChatMessageEntity>(cancellationToken: cancellationToken))
        {
            messages.Add(new ChatMessageDto(
                Guid.Parse(entity.RowKey),
                entity.Sender,
                entity.Message,
                entity.CreatedOn
            ));
        }

        // Return messages in descending order by CreatedOn
        return messages.OrderByDescending(m => m.CreatedOn);
    }

    private async Task CreateIfNotExistsAsync(CancellationToken cancellationToken = default)
    {
        if (!_tableInitialized)
        {
            try
            {
                // Try to create the table
                await _tableClient.CreateAsync(cancellationToken);
                _tableInitialized = true;
            }
            catch (RequestFailedException ex) when (ex.Status == 409)
            {
                // Table already exists, that's fine
                _tableInitialized = true;
            }
        }
    }
}

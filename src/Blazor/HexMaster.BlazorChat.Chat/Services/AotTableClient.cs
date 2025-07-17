using Azure.Data.Tables;
using HexMaster.BlazorChat.Chat.Entities;
using System.Text.Json;
using Azure;

namespace HexMaster.BlazorChat.Chat.Services;

public class AotTableClient
{
    private readonly TableClient _tableClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private bool _tableInitialized = false;

    public AotTableClient(TableServiceClient tableServiceClient, string tableName, JsonSerializerOptions jsonOptions)
    {
        _tableClient = tableServiceClient.GetTableClient(tableName);
        _jsonOptions = jsonOptions;
    }

    public async Task CreateIfNotExistsAsync(CancellationToken cancellationToken = default)
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

    public async Task AddEntityAsync(ChatMessageEntity entity, CancellationToken cancellationToken = default)
    {
        // Ensure table exists first
        await CreateIfNotExistsAsync(cancellationToken);

        // Create TableEntity directly without reflection
        var tableEntity = new TableEntity(entity.PartitionKey, entity.RowKey)
        {
            ["Id"] = entity.Id,
            ["Sender"] = entity.Sender,
            ["Message"] = entity.Message,
            ["CreatedOn"] = entity.CreatedOn
        };

        await _tableClient.AddEntityAsync(tableEntity, cancellationToken);
    }
}

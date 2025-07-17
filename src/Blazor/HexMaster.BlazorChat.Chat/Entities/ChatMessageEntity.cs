using Azure;
using Azure.Data.Tables;

namespace HexMaster.BlazorChat.Chat.Entities;

public class ChatMessageEntity : ITableEntity
{
    public string PartitionKey { get; set; } = "ChatMessages";
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    
    public Guid Id { get; set; }
    public string Sender { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTimeOffset CreatedOn { get; set; }

    public ChatMessageEntity()
    {
    }

    public ChatMessageEntity(Guid id, string sender, string message, DateTimeOffset createdOn)
    {
        PartitionKey = "ChatMessages";
        RowKey = id.ToString();
        Id = id;
        Sender = sender;
        Message = message;
        CreatedOn = createdOn;
        ETag = ETag.All;
    }
}

using HexMaster.BlazorChat.Chat.Abstractions.DataTransferObjects;
using System.Text.Json.Serialization;

namespace HexMaster.BlazorChat.Chat;

[JsonSerializable(typeof(CreateChatMessageRequest))]
[JsonSerializable(typeof(CreateChatMessageResponse))]
[JsonSerializable(typeof(ChatMessageDto))]
[JsonSerializable(typeof(IEnumerable<ChatMessageDto>))]
public partial class ChatMessageSerializationContext : JsonSerializerContext
{
}



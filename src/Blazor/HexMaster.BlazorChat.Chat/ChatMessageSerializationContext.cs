using HexMaster.BlazorChat.Chat.Abstractions.DataTransferObjects;
using System.Text.Json.Serialization;

namespace HexMaster.BlazorChat.Chat;

[JsonSerializable(typeof(CreateChatMessageRequest))]
[JsonSerializable(typeof(CreateChatMessageResponse))]
public partial class ChatMessageSerializationContext : JsonSerializerContext
{
}



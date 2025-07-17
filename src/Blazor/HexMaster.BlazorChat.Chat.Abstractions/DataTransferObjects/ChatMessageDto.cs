namespace HexMaster.BlazorChat.Chat.Abstractions.DataTransferObjects;

public record ChatMessageDto(Guid Id, string Sender, string Message, DateTimeOffset CreatedOn);

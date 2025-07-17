using HexMaster.BlazorChat.Chat.Abstractions.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexMaster.BlazorChat.Chat.Abstractions.Services;

public interface IChatMessagesService
{
    ValueTask<CreateChatMessageResponse> CreateMessage(CreateChatMessageRequest request, CancellationToken cancellationToken);
    ValueTask<IEnumerable<ChatMessageDto>> GetMessagesAsync(CancellationToken cancellationToken);
}

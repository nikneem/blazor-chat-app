using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexMaster.BlazorChat.Chat.Abstractions.DataTransferObjects;

public record CreateChatMessageResponse(Guid Id, string Sender, string Message, DateTimeOffset CreatedOn);

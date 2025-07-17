using HexMaster.BlazorChat.Chat.Abstractions.DataTransferObjects;
using System.Text.Json;

namespace HexMaster.BlazorChat.Client.Services;

public interface IChatClientService
{
    Task<IEnumerable<ChatMessageDto>> GetMessagesAsync();
    Task<CreateChatMessageResponse> SendMessageAsync(CreateChatMessageRequest request);
}

public class ChatClientService : IChatClientService
{
    private readonly HttpClient _httpClient;

    public ChatClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<ChatMessageDto>> GetMessagesAsync()
    {
        var response = await _httpClient.GetAsync("chat/messages");
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<ChatMessageDto>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? Enumerable.Empty<ChatMessageDto>();
    }

    public async Task<CreateChatMessageResponse> SendMessageAsync(CreateChatMessageRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("chat/messages", request);
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<CreateChatMessageResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
    }
}
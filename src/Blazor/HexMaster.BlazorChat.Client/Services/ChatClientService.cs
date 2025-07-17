using HexMaster.BlazorChat.Chat.Abstractions.DataTransferObjects;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace HexMaster.BlazorChat.Client.Services;

public interface IChatClientService
{
    Task<IEnumerable<ChatMessageDto>> GetMessagesAsync();
    Task<CreateChatMessageResponse> SendMessageAsync(CreateChatMessageRequest request);
    Task StartConnectionAsync();
    Task StopConnectionAsync();
    Task JoinChatAsync(string username);
    Task LeaveChatAsync(string username);
    event Action<ChatMessageDto>? MessageReceived;
    event Action<string>? UserJoined;
    event Action<string>? UserLeft;
    bool IsConnected { get; }
}

public class ChatClientService(IHttpClientFactory factory, IConfiguration configuration) : IChatClientService, IAsyncDisposable
{
    private readonly HttpClient _httpClient = factory.CreateClient("ChatApi");
    private readonly IConfiguration _configuration = configuration;
    private HubConnection? _hubConnection;

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public event Action<ChatMessageDto>? MessageReceived;
    public event Action<string>? UserJoined;
    public event Action<string>? UserLeft;

    public async Task StartConnectionAsync()
    {
        if (_hubConnection is not null)
            return;

        // For SignalR, we need to resolve the actual server URL, not the service discovery name
        string hubUrl;
        
        try
        {
            // First, try to make an HTTP request to get the actual server URL
            var response = await _httpClient.GetAsync("chat/messages");
            var actualBaseUrl = response.RequestMessage?.RequestUri?.GetLeftPart(UriPartial.Authority);
            
            if (!string.IsNullOrEmpty(actualBaseUrl))
            {
                hubUrl = $"{actualBaseUrl}/chathub";
            }
            else
            {
                // Fallback: try to construct from base address
                var baseAddress = _httpClient.BaseAddress?.ToString();
                if (!string.IsNullOrEmpty(baseAddress))
                {
                    hubUrl = new Uri(new Uri(baseAddress), "/chathub").ToString();
                }
                else
                {
                    throw new InvalidOperationException("Unable to determine SignalR hub URL");
                }
            }
        }
        catch (Exception)
        {
            // If HTTP call fails, try using the base address directly
            var baseAddress = _httpClient.BaseAddress?.ToString() ?? "https://localhost:7001";
            hubUrl = new Uri(new Uri(baseAddress), "/chathub").ToString();
        }
        
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .Build();

        _hubConnection.On<CreateChatMessageResponse>("ReceiveMessage", (message) =>
        {
            var dto = new ChatMessageDto(message.Id, message.Sender, message.Message, message.CreatedOn);
            MessageReceived?.Invoke(dto);
        });

        _hubConnection.On<string>("UserJoined", (username) =>
        {
            UserJoined?.Invoke(username);
        });

        _hubConnection.On<string>("UserLeft", (username) =>
        {
            UserLeft?.Invoke(username);
        });

        await _hubConnection.StartAsync();
    }

    public async Task StopConnectionAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.DisposeAsync();
            _hubConnection = null;
        }
    }

    public async Task JoinChatAsync(string username)
    {
        if (_hubConnection is not null && _hubConnection.State == HubConnectionState.Connected)
        {
            await _hubConnection.SendAsync("JoinChat", username);
        }
    }

    public async Task LeaveChatAsync(string username)
    {
        if (_hubConnection is not null && _hubConnection.State == HubConnectionState.Connected)
        {
            await _hubConnection.SendAsync("LeaveChat", username);
        }
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

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}
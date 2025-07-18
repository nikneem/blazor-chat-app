using HexMaster.BlazorChat.Chat.Abstractions.DataTransferObjects;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Http.Connections;
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

public class ChatClientService(IHttpClientFactory factory) : IChatClientService, IAsyncDisposable
{
    private readonly HttpClient _httpClient = factory.CreateClient("ChatApi");
    private HubConnection? _hubConnection;

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public event Action<ChatMessageDto>? MessageReceived;
    public event Action<string>? UserJoined;
    public event Action<string>? UserLeft;

    public async Task StartConnectionAsync()
    {
        if (_hubConnection is not null)
        {
            Console.WriteLine("SignalR: Connection already exists");
            return;
        }

        // For SignalR, we need to resolve the actual server URL, not the service discovery name
        string hubUrl;
        
        try
        {
            Console.WriteLine("SignalR: Attempting to resolve server URL...");
            // First, try to make an HTTP request to get the actual server URL
            var response = await _httpClient.GetAsync("chat/messages");
            var actualBaseUrl = response.RequestMessage?.RequestUri?.GetLeftPart(UriPartial.Authority);
            
            Console.WriteLine($"SignalR: Resolved base URL: {actualBaseUrl}");
            
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
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR: Error resolving server URL: {ex.Message}");
            // If HTTP call fails, try using the base address directly
            var baseAddress = _httpClient.BaseAddress?.ToString() ?? "http://hexmaster-blazorchat-server";
            hubUrl = new Uri(new Uri(baseAddress), "/chathub").ToString();
        }
        
        Console.WriteLine($"SignalR: Final hub URL: {hubUrl}");
        Console.WriteLine($"SignalR: Building connection...");
        
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                // Add detailed logging for transport
                Console.WriteLine($"SignalR: Configuring connection options for {hubUrl}");
                
                // Try with all transports to see which ones work
                options.Transports = HttpTransportType.WebSockets | HttpTransportType.ServerSentEvents | HttpTransportType.LongPolling;
                
                // Add headers for debugging
                options.Headers.Add("X-Requested-With", "SignalR");
            })
            .WithAutomaticReconnect()
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Debug);
            })
            .Build();

        // Add connection state event handlers
        _hubConnection.Closed += (error) =>
        {
            Console.WriteLine($"SignalR: Connection closed. Error: {error?.Message ?? "None"}");
            Console.WriteLine($"SignalR: Error details: {error}");
            Console.WriteLine($"SignalR: Connection state: {_hubConnection?.State}");
            return Task.CompletedTask;
        };

        _hubConnection.Reconnected += (connectionId) =>
        {
            Console.WriteLine($"SignalR: Reconnected with connection ID: {connectionId}");
            return Task.CompletedTask;
        };

        _hubConnection.Reconnecting += (error) =>
        {
            Console.WriteLine($"SignalR: Reconnecting... Error: {error?.Message ?? "None"}");
            return Task.CompletedTask;
        };

        Console.WriteLine("SignalR: Setting up event handlers...");

        // Simple test handler
        _hubConnection.On<string>("TestMessage", (msg) =>
        {
            Console.WriteLine($"SignalR: TestMessage received: {msg}");
        });

        // Handler for individual parameters (converted to strings)
        _hubConnection.On<string, string, string, string>("ReceiveMessage", (id, sender, message, createdOn) =>
        {
            Console.WriteLine($"SignalR: ReceiveMessage (string params) triggered! ID: {id}, Sender: {sender}, Message: {message}");
            try
            {
                var guidId = Guid.Parse(id);
                var dateTimeOffset = DateTimeOffset.Parse(createdOn);
                var dto = new ChatMessageDto(guidId, sender, message, dateTimeOffset);
                Console.WriteLine($"SignalR: Invoking MessageReceived event. Subscribers: {MessageReceived?.GetInvocationList().Length ?? 0}");
                MessageReceived?.Invoke(dto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignalR: Error parsing message parameters: {ex.Message}");
            }
        });

        // Handler for individual parameters (this should fire now!)
        _hubConnection.On<Guid, string, string, DateTimeOffset>("ReceiveMessage", (id, sender, message, createdOn) =>
        {
            Console.WriteLine($"SignalR: ReceiveMessage (individual params) triggered! ID: {id}, Sender: {sender}, Message: {message}");
            var dto = new ChatMessageDto(id, sender, message, createdOn);
            Console.WriteLine($"SignalR: Invoking MessageReceived event. Subscribers: {MessageReceived?.GetInvocationList().Length ?? 0}");
            MessageReceived?.Invoke(dto);
        });

        // Keep these for debugging purposes
        _hubConnection.On("ReceiveMessage", () => {
            Console.WriteLine("SignalR: ReceiveMessage (no parameters) triggered");
        });

        _hubConnection.On<object>("ReceiveMessage", (rawMessage) =>
        {
            Console.WriteLine($"SignalR: ReceiveMessage (object) triggered. Type: {rawMessage?.GetType().Name}");
        });

        _hubConnection.On<CreateChatMessageResponse>("ReceiveMessage", (message) =>
        {
            Console.WriteLine($"SignalR: ReceiveMessage (CreateChatMessageResponse) triggered for: {message.Message}");
            var dto = new ChatMessageDto(message.Id, message.Sender, message.Message, message.CreatedOn);
            MessageReceived?.Invoke(dto);
        });

        _hubConnection.On<string>("UserJoined", (username) =>
        {
            Console.WriteLine($"SignalR: UserJoined event triggered for: {username}");
            UserJoined?.Invoke(username);
        });

        _hubConnection.On<string>("UserLeft", (username) =>
        {
            Console.WriteLine($"SignalR: UserLeft event triggered for: {username}");
            UserLeft?.Invoke(username);
        });

        try
        {
            Console.WriteLine($"SignalR: Starting connection to {hubUrl}");
            await _hubConnection.StartAsync();
            Console.WriteLine($"SignalR: Connection started successfully. State: {_hubConnection.State}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR: Failed to start connection: {ex.Message}");
            Console.WriteLine($"SignalR: Exception details: {ex}");
            throw;
        }
    }

    public async Task StopConnectionAsync()
    {
        if (_hubConnection is not null)
        {
            Console.WriteLine("SignalR: Stopping connection");
            await _hubConnection.DisposeAsync();
            _hubConnection = null;
            Console.WriteLine("SignalR: Connection stopped");
        }
    }

    public async Task JoinChatAsync(string username)
    {
        if (_hubConnection is not null && _hubConnection.State == HubConnectionState.Connected)
        {
            Console.WriteLine($"SignalR: Joining chat as {username}");
            await _hubConnection.SendAsync("JoinChat", username);
            Console.WriteLine($"SignalR: Successfully joined chat");
        }
        else
        {
            Console.WriteLine($"SignalR: Cannot join chat - connection state: {_hubConnection?.State ?? HubConnectionState.Disconnected}");
        }
    }

    public async Task LeaveChatAsync(string username)
    {
        if (_hubConnection is not null && _hubConnection.State == HubConnectionState.Connected)
        {
            Console.WriteLine($"SignalR: Leaving chat as {username}");
            await _hubConnection.SendAsync("LeaveChat", username);
            Console.WriteLine($"SignalR: Successfully left chat");
        }
        else
        {
            Console.WriteLine($"SignalR: Cannot leave chat - connection state: {_hubConnection?.State ?? HubConnectionState.Disconnected}");
        }
    }

    public async Task<IEnumerable<ChatMessageDto>> GetMessagesAsync()
    {
        try
        {
            Console.WriteLine("HTTP: Fetching messages...");
            var response = await _httpClient.GetAsync("chat/messages");
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            var messages = JsonSerializer.Deserialize<IEnumerable<ChatMessageDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? Enumerable.Empty<ChatMessageDto>();
            
            Console.WriteLine($"HTTP: Retrieved {messages.Count()} messages");
            return messages;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"HTTP: Error fetching messages: {ex.Message}");
            throw;
        }
    }

    public async Task<CreateChatMessageResponse> SendMessageAsync(CreateChatMessageRequest request)
    {
        try
        {
            Console.WriteLine($"HTTP: Sending message: {request.Message}");
            var response = await _httpClient.PostAsJsonAsync("chat/messages", request);
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<CreateChatMessageResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!;
            
            Console.WriteLine($"HTTP: Message sent successfully with ID: {result.Id}");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"HTTP: Error sending message: {ex.Message}");
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}
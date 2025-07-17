using HexMaster.BlazorChat.Client.Components;
using HexMaster.BlazorChat.Client.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add HttpClient with named configuration for chat service
builder.Services.AddHttpClient("ChatApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:5111/");
});

// Register ChatClientService as singleton to maintain SignalR connection state
builder.Services.AddSingleton<IChatClientService, ChatClientService>();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

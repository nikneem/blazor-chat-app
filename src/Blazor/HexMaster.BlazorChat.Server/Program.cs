using HexMaster.BlazorChat.Chat.ExtensionMethods;
using HexMaster.BlazorChat.Server.Endpoints.Chat;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(args);

        builder.AddServiceDefaults();
        builder.AddChatMessages();


        var app = builder.Build();

        app.MapDefaultEndpoints().MapChatEndpoints();

        app.Run();
    }
}
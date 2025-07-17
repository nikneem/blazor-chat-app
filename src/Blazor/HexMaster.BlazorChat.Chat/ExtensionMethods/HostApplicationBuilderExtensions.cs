
using HexMaster.BlazorChat.Chat.Abstractions.Repositories;
using HexMaster.BlazorChat.Chat.Abstractions.Services;
using HexMaster.BlazorChat.Chat.Repositories;
using HexMaster.BlazorChat.Chat.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HexMaster.BlazorChat.Chat.ExtensionMethods;

public static  class HostApplicationBuilderExtensions
{
    public static TBuilder AddChatMessages<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddScoped<IChatMessagesRepository, ChatMessagesRepository>();
        builder.Services.AddScoped<IChatMessagesService, ChatMessagesService>();

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, ChatMessageSerializationContext.Default);
        });

        return builder;
    }
}

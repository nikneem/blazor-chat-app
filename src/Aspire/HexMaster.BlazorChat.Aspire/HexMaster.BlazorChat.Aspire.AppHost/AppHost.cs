var builder = DistributedApplication.CreateBuilder(args);

// Add Azure Storage with emulator for local development
var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator(em=>
    {
        em.WithLifetime(ContainerLifetime.Persistent);
    });

// Add Table Storage for chat messages
var chatMessagesTable = storage.AddTables("chatmessages");

var serverApi = builder
    .AddProject<Projects.HexMaster_BlazorChat_Server>("hexmaster-blazorchat-server")
    .WithReference(chatMessagesTable);

builder.AddProject<Projects.HexMaster_BlazorChat_Client>("hexmaster-blazorchat-client")
    .WaitFor(serverApi)
    .WithReference(serverApi);

builder.Build().Run();

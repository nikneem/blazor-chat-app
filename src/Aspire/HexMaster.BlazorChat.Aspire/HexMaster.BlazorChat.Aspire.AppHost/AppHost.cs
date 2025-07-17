var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.HexMaster_BlazorChat_Client>("hexmaster-blazorchat-client");

builder.AddProject<Projects.HexMaster_BlazorChat_Server>("hexmaster-blazorchat-server");

builder.Build().Run();

var builder = DistributedApplication.CreateBuilder(args);

var sqlServer = builder.AddSqlServer("t3CloneSqlserver", port: 1433)
    .WithLifetime(ContainerLifetime.Persistent);

var database = sqlServer
    .AddDatabase("t3CloneDb");

var apiService = builder
    .AddProject<Projects.LLMLab_Server>("t3CloneServer", "http")
    .WithReference(database)
    .WaitFor(database);

builder.AddProject<Projects.LLMLab_Client>("t3CloneClient", "https")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();

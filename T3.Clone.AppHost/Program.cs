var builder = DistributedApplication.CreateBuilder(args);

var sqlServer = builder.AddSqlServer("t3CloneSqlserver", port: 1433)
    .WithLifetime(ContainerLifetime.Persistent);

var database = sqlServer
    .AddDatabase("t3CloneDb");
    
var cache = builder.AddRedis("cache");

var apiService = builder
    .AddProject<Projects.T3_Clone_Server>("t3CloneServer", "http");
    // .WithReference(database)
    // .WaitFor(database);

builder.AddProject<Projects.T3_Clone_Client>("t3CloneClient", "https")
    .WithExternalHttpEndpoints()
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();

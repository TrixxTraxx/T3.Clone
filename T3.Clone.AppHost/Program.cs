var builder = DistributedApplication.CreateBuilder(args);

var database = builder.AddSqlServer("t3CloneSqlserver", port: 1433)
    .WithDataVolume("T3CloneDataVolume");

var cache = builder.AddRedis("cache");

var apiService = builder
    .AddProject<Projects.T3_Clone_Server>("t3CloneServer")
    .WithReference(database)
    .WaitFor(database);

builder.AddProject<Projects.T3_Clone_Client>("t3CloneClient")
    .WithExternalHttpEndpoints()
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();

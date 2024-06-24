var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis");
var postgres = builder.AddPostgres("postgres");
var postgresdb = postgres.AddDatabase("postgresdb");

builder.AddProject<Projects.Ticketing>("webapi")
    .WithReference(redis)
    .WithReference(postgresdb)
    .WithExternalHttpEndpoints();

builder.Build().Run();

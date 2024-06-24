using System.Text.Json.Serialization;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Ticketing;
using Ticketing.Data;
using Ticketing.Settings;
using Vernou.Swashbuckle.HttpResultsAdapter;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

var postgresConnectionString = builder.Configuration.GetValue<string>("POSTGRESQLCONNSTR_DatabaseConnection");
var redisConnectionString = builder.Configuration.GetValue<string>("RedisCacheCONNSTR_RedisConnection");
var queueName = builder.Configuration.GetValue<string>("ServiceBusSettings:QueueName");
var serviceBusNamespace = builder.Configuration.GetValue<string>("ServiceBusCONNSTR_ServiceBusConnection");
var cacheExpiration = builder.Configuration.GetValue<int>("CacheExpirationInMinutes");

builder.Services.Configure<ServiceBusSettings>(options =>
    builder.Configuration.GetSection("ServiceBusSettings").Bind(options));

builder.Services.Configure<CartItemsExpiration>(options =>
    builder.Configuration.GetSection("AppSettings").Bind(options));

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddResponseCaching();

builder.Services.AddSwaggerGen(c =>
{
    c.TagActionsBy(api =>
    {
        if (api.GroupName != null)
        {
            return new[] { api.GroupName };
        }

        if (api.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
        {
            return new[] { controllerActionDescriptor.ControllerName };
        }

        throw new InvalidOperationException("Unable to determine tag for endpoint.");
    });

    c.DocInclusionPredicate((name, api) => true);

    c.OperationFilter<HttpResultsOperationFilter>();
});

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

if (builder.Environment.IsDevelopment())
{
    builder.AddNpgsqlDbContext<TicketingDbContext>("postgresdb");
    builder.Services.AddOutputCache(opt => opt.DefaultExpirationTimeSpan = TimeSpan.FromMinutes(cacheExpiration));
}
else
{
    builder.Services.AddDbContextPool<TicketingDbContext>(x => { x.UseNpgsql(postgresConnectionString); });
    builder.Services
        .AddOutputCache(opt => opt.DefaultExpirationTimeSpan = TimeSpan.FromMinutes(cacheExpiration))
        .AddStackExchangeRedisOutputCache(options => { options.Configuration = redisConnectionString; });
}

builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder
        .AddServiceBusClientWithNamespace(serviceBusNamespace)
        .WithCredential(new DefaultAzureCredential())
        .ConfigureOptions(clientOptions =>
        {
            clientOptions.RetryOptions.Mode = ServiceBusRetryMode.Exponential;
            clientOptions.RetryOptions.MaxRetries =
                builder.Configuration.GetValue<int>("ServiceBusSettings:MaxRetries");
            clientOptions.RetryOptions.TryTimeout =
                TimeSpan.FromSeconds(builder.Configuration.GetValue<int>("ServiceBusSettings:TryTimeout"));
        });

    clientBuilder.AddClient<ServiceBusSender, ServiceBusClientOptions>((_, _, provider) =>
        provider.GetService<ServiceBusClient>()?.CreateSender(queueName)!).WithName(queueName);
});

builder.Services.AddHealthChecks()
    .AddNpgSql(postgresConnectionString!)
    .AddRedis(redisConnectionString!)
    .AddAzureServiceBusQueue(serviceBusNamespace!, queueName!, new DefaultAzureCredential());

var app = builder.Build();

app.UseResponseCaching();

if (app.Environment.IsDevelopment())
{
    await app.SeedData();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseExceptionHandler();

app.UseHealthChecks("/_health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseOutputCache();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;

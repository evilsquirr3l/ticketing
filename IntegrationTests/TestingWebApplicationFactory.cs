using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.OutputCaching.StackExchangeRedis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ticketing.Data;

namespace IntegrationTests;

public class TestingWebApplicationFactory(string databaseConnectionString, string redisConnectionString)
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            SwapDbContext(services);

            SwapRedis(services);

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            using var appContext = scope.ServiceProvider.GetRequiredService<TicketingDbContext>();

            appContext.Database.EnsureCreated();
        });
    }

    private void SwapRedis(IServiceCollection services)
    {
        var redis = services.SingleOrDefault(
            d => d.ServiceType ==
                 typeof(RedisOutputCacheOptions));

        if (redis is not null)
        {
            services.Remove(redis);
        }

        services.AddOutputCache(opt =>
                opt.DefaultExpirationTimeSpan = TimeSpan.FromMilliseconds(1))
            .AddStackExchangeRedisOutputCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "Ticketing";
            });
    }

    private void SwapDbContext(IServiceCollection services)
    {
        var dbcontext = services.SingleOrDefault(
            d => d.ServiceType ==
                 typeof(DbContextOptions<TicketingDbContext>));

        if (dbcontext is not null)
        {
            services.Remove(dbcontext);
        }

        services.AddDbContextPool<TicketingDbContext>(options =>
            options.UseNpgsql(databaseConnectionString));
    }
}

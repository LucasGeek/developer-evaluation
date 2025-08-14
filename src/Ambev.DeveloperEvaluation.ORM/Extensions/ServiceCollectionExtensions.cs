using Ambev.DeveloperEvaluation.ORM.MongoDB;
using Ambev.DeveloperEvaluation.ORM.Redis;
using Ambev.DeveloperEvaluation.ORM.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Rebus.Config;
using Rebus.PostgreSql.Transport;
using Rebus.PostgreSql.Subscriptions;
using Rebus.Routing.TypeBased;
using Rebus.ServiceProvider;

namespace Ambev.DeveloperEvaluation.ORM.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IMongoContext, MongoContext>();
        return services;
    }

    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("RedisConnection") 
            ?? throw new ArgumentNullException("RedisConnection");
            
        services.AddSingleton<IConnectionMultiplexer>(provider =>
            ConnectionMultiplexer.Connect(connectionString));
            
        services.AddSingleton<ICacheService, RedisCacheService>();
        
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connectionString;
        });
        
        return services;
    }

    public static IServiceCollection AddRebusMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException("DefaultConnection");

        services.AddRebus(configure => configure
            .Transport(t => t.UsePostgreSql(connectionString, "messages"))
            .Subscriptions(s => s.UsePostgreSql(connectionString, "subscriptions"))
            .Routing(r => r.TypeBased().MapAssemblyOf<Program>("messages"))
            .Options(o => o.SetMaxParallelism(1))
        );

        services.AddScoped<IEventBus, RebusEventBus>();
        
        return services;
    }

    public static IServiceCollection AddCQRSInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMongoDb(configuration);
        services.AddRedisCache(configuration);
        services.AddRebusMessaging(configuration);
        
        return services;
    }
}
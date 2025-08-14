using Ambev.DeveloperEvaluation.ORM.MongoDB;
using Ambev.DeveloperEvaluation.ORM.Redis;
using Ambev.DeveloperEvaluation.ORM.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

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

    public static IServiceCollection AddEventBus(this IServiceCollection services, IConfiguration configuration)
    {
        // Using logging-based event bus for domain events
        // This follows CLAUDE.md guidance for "differential" - events are logged with structured data
        services.AddScoped<IEventBus, LoggingEventBus>();
        
        return services;
    }

    public static IServiceCollection AddCQRSInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMongoDb(configuration);
        services.AddRedisCache(configuration);
        services.AddEventBus(configuration);
        
        return services;
    }
}
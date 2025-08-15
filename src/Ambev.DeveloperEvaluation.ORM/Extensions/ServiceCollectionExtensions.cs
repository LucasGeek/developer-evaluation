using Ambev.DeveloperEvaluation.ORM.MongoDB;
using Ambev.DeveloperEvaluation.ORM.MongoDB.Repositories;
using Ambev.DeveloperEvaluation.ORM.MongoDB.ReadModels;
using Ambev.DeveloperEvaluation.ORM.Redis;
using Ambev.DeveloperEvaluation.ORM.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Rebus.Config;

namespace Ambev.DeveloperEvaluation.ORM.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
    {
        // Add MongoDB only if connection string is available
        var connectionString = configuration.GetConnectionString("MongoDbConnection");
        if (!string.IsNullOrEmpty(connectionString))
        {
            services.AddSingleton<IMongoContext, MongoContext>();
            
            // Register read model repositories
            services.AddScoped<IReadModelRepository<SaleReadModel>, MongoReadModelRepository<SaleReadModel>>();
            services.AddScoped<IReadModelRepository<ProductReadModel>, MongoReadModelRepository<ProductReadModel>>();
            services.AddScoped<IReadModelRepository<UserReadModel>, MongoReadModelRepository<UserReadModel>>();
        }
        else
        {
            // Add null object pattern repositories for development
            services.AddScoped<IReadModelRepository<SaleReadModel>, NullReadModelRepository<SaleReadModel>>();
            services.AddScoped<IReadModelRepository<ProductReadModel>, NullReadModelRepository<ProductReadModel>>();
            services.AddScoped<IReadModelRepository<UserReadModel>, NullReadModelRepository<UserReadModel>>();
        }
        return services;
    }

    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Redis only if connection string is available
        var connectionString = configuration.GetConnectionString("RedisConnection");
        if (!string.IsNullOrEmpty(connectionString))
        {
            services.AddSingleton<IConnectionMultiplexer>(provider =>
                ConnectionMultiplexer.Connect(connectionString));
                
            services.AddSingleton<ICacheService, RedisCacheService>();
            
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = connectionString;
            });
        }
        else
        {
            // Add a no-op cache service for development
            services.AddSingleton<ICacheService, NullCacheService>();
        }
        
        return services;
    }

    public static IServiceCollection AddEventBus(this IServiceCollection services, IConfiguration configuration)
    {
        // Use MediatR-based event bus for proper CQRS event handling
        // This enables domain event handlers to update read models
        services.AddScoped<IEventBus, MediatREventBus>();
        
        // Rebus event bus is temporarily disabled until proper transport is configured
        // services.AddScoped<RebusEventBus>();
        
        return services;
    }

    public static IServiceCollection AddRebus(this IServiceCollection services, IConfiguration configuration)
    {
        // For now, we'll use MediatR for event publishing
        // In production, this could be extended with proper Rebus transport (Redis, RabbitMQ, etc.)
        // services.AddRebus(configure => configure...);
        
        return services;
    }

    public static IServiceCollection AddCQRSInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMongoDb(configuration);
        services.AddRedisCache(configuration);
        services.AddEventBus(configuration);
        services.AddRebus(configuration);
        
        return services;
    }
}
using DeveloperEvaluation.ORM.MongoDB;
using DeveloperEvaluation.ORM.MongoDB.Repositories;
using DeveloperEvaluation.ORM.MongoDB.ReadModels;
using DeveloperEvaluation.ORM.Redis;
using DeveloperEvaluation.ORM.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Rebus.Config;
using Serilog;

namespace DeveloperEvaluation.ORM.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
    {
        
        var connectionString = configuration.GetConnectionString("MongoDbConnection");
        
        if (!string.IsNullOrEmpty(connectionString))
        {
            try
            {
                services.AddSingleton<IMongoContext, MongoContext>();
                
                services.AddScoped<IReadModelRepository<SaleReadModel>, MongoReadModelRepository<SaleReadModel>>();
                services.AddScoped<IReadModelRepository<ProductReadModel>, MongoReadModelRepository<ProductReadModel>>();
                services.AddScoped<IReadModelRepository<UserReadModel>, MongoReadModelRepository<UserReadModel>>();
                
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ [MONGO] MongoDB setup failed, falling back to null repositories");
                services.AddScoped<IReadModelRepository<SaleReadModel>, NullReadModelRepository<SaleReadModel>>();
                services.AddScoped<IReadModelRepository<ProductReadModel>, NullReadModelRepository<ProductReadModel>>();
                services.AddScoped<IReadModelRepository<UserReadModel>, NullReadModelRepository<UserReadModel>>();
            }
        }
        else
        {
            services.AddScoped<IReadModelRepository<SaleReadModel>, NullReadModelRepository<SaleReadModel>>();
            services.AddScoped<IReadModelRepository<ProductReadModel>, NullReadModelRepository<ProductReadModel>>();
            services.AddScoped<IReadModelRepository<UserReadModel>, NullReadModelRepository<UserReadModel>>();
        }
        return services;
    }

    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        
        var connectionString = configuration.GetConnectionString("RedisConnection");
        
        if (!string.IsNullOrEmpty(connectionString))
        {
            try
            {
                
                services.AddSingleton<IConnectionMultiplexer>(provider =>
                {
                    try
                    {
                        var connection = ConnectionMultiplexer.Connect(connectionString);
                        return connection;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "❌ [REDIS] Redis connection failed, returning null");
                        return null;
                    }
                });
                    
                services.AddSingleton<ICacheService, RedisCacheService>();
                
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = connectionString;
                });
                
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ [REDIS] Redis setup failed, falling back to null cache service");
                services.AddSingleton<ICacheService, NullCacheService>();
            }
        }
        else
        {
            services.AddSingleton<ICacheService, NullCacheService>();
        }
        
        return services;
    }

    public static IServiceCollection AddEventBus(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IEventBus, MediatREventBus>();
        
        
        return services;
    }

    public static IServiceCollection AddRebus(this IServiceCollection services, IConfiguration configuration)
    {
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
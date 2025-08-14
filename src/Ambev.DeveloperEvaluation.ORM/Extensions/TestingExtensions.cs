using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Ambev.DeveloperEvaluation.ORM.MongoDB.Repositories;
using Ambev.DeveloperEvaluation.ORM.MongoDB.ReadModels;
using Ambev.DeveloperEvaluation.ORM.Redis;
using Ambev.DeveloperEvaluation.ORM.Messaging;

namespace Ambev.DeveloperEvaluation.ORM.Extensions;

/// <summary>
/// Extensions for configuring testing environments with in-memory databases
/// </summary>
public static class TestingExtensions
{
    /// <summary>
    /// Configures the application to use in-memory databases for testing
    /// This provides a complete CQRS setup without external dependencies
    /// </summary>
    public static IServiceCollection AddInMemoryDatabases(this IServiceCollection services)
    {
        // Configure EF Core with In-Memory database for write models
        services.AddDbContext<DefaultContext>(options =>
            options.UseInMemoryDatabase("TestDb"));

        // Use in-memory implementations for read models (no MongoDB required)
        services.AddScoped<IReadModelRepository<SaleReadModel>, NullReadModelRepository<SaleReadModel>>();
        services.AddScoped<IReadModelRepository<ProductReadModel>, NullReadModelRepository<ProductReadModel>>();
        services.AddScoped<IReadModelRepository<UserReadModel>, NullReadModelRepository<UserReadModel>>();

        // Use null cache service (no Redis required)
        services.AddSingleton<ICacheService, NullCacheService>();

        // Use logging event bus for testing
        services.AddScoped<IEventBus, LoggingEventBus>();

        return services;
    }

    /// <summary>
    /// Configures the application to use SQLite databases for integration testing
    /// This provides persistent storage during test runs
    /// </summary>
    public static IServiceCollection AddSQLiteDatabases(this IServiceCollection services, string connectionString = "Data Source=:memory:")
    {
        // Configure EF Core with SQLite for write models
        services.AddDbContext<DefaultContext>(options =>
            options.UseSqlite(connectionString));

        // Use in-memory implementations for read models (no MongoDB required)
        services.AddScoped<IReadModelRepository<SaleReadModel>, NullReadModelRepository<SaleReadModel>>();
        services.AddScoped<IReadModelRepository<ProductReadModel>, NullReadModelRepository<ProductReadModel>>();
        services.AddScoped<IReadModelRepository<UserReadModel>, NullReadModelRepository<UserReadModel>>();

        // Use null cache service (no Redis required)
        services.AddSingleton<ICacheService, NullCacheService>();

        // Use logging event bus for testing
        services.AddScoped<IEventBus, LoggingEventBus>();

        return services;
    }
}
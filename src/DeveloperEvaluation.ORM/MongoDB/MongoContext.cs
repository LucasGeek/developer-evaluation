using MongoDB.Driver;
using Microsoft.Extensions.Configuration;

namespace DeveloperEvaluation.ORM.MongoDB;

public class MongoContext : IMongoContext
{
    public IMongoDatabase Database { get; }

    public MongoContext(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MongoDbConnection") 
            ?? throw new ArgumentNullException(nameof(configuration));
            
        var client = new MongoClient(connectionString);
        Database = client.GetDatabase("DeveloperEvaluationReadModel");
    }

    public IMongoCollection<T> GetCollection<T>(string? name = null)
    {
        name ??= typeof(T).Name.ToLowerInvariant();
        return Database.GetCollection<T>(name);
    }
}
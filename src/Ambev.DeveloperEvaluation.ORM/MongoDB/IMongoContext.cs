using MongoDB.Driver;

namespace Ambev.DeveloperEvaluation.ORM.MongoDB;

public interface IMongoContext
{
    IMongoDatabase Database { get; }
    IMongoCollection<T> GetCollection<T>(string? name = null);
}
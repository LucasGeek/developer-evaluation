using MongoDB.Driver;
using DeveloperEvaluation.Domain.Common;
using DeveloperEvaluation.ORM.MongoDB.ReadModels;

namespace DeveloperEvaluation.ORM.MongoDB.Repositories;

public class MongoReadModelRepository<T> : IReadModelRepository<T> where T : class
{
    private readonly IMongoCollection<T> _collection;

    public MongoReadModelRepository(IMongoContext mongoContext)
    {
        _collection = mongoContext.GetCollection<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<T>.Filter.Eq("Id", id);
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PaginatedList<T>> GetAllAsync(int page = 1, int limit = 10, CancellationToken cancellationToken = default)
    {
        var totalCount = await _collection.CountDocumentsAsync(FilterDefinition<T>.Empty, cancellationToken: cancellationToken);
        
        var skip = (page - 1) * limit;
        var items = await _collection
            .Find(FilterDefinition<T>.Empty)
            .Skip(skip)
            .Limit(limit)
            .ToListAsync(cancellationToken);

        return new PaginatedList<T>(items, (int)totalCount, page, limit);
    }

    public async Task CreateAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty != null)
        {
            var id = idProperty.GetValue(entity);
            var filter = Builders<T>.Filter.Eq("Id", id);
            await _collection.ReplaceOneAsync(filter, entity, cancellationToken: cancellationToken);
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<T>.Filter.Eq("Id", id);
        await _collection.DeleteOneAsync(filter, cancellationToken);
    }
}
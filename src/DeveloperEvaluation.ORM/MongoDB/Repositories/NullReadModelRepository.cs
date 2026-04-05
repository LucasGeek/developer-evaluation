using DeveloperEvaluation.Domain.Common;

namespace DeveloperEvaluation.ORM.MongoDB.Repositories;

/// <summary>
/// Null object pattern implementation of IReadModelRepository for development environments
/// where MongoDB is not available
/// </summary>
public class NullReadModelRepository<T> : IReadModelRepository<T> where T : class
{
    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<T?>(null);
    }

    public Task<PaginatedList<T>> GetAllAsync(int page = 1, int limit = 10, CancellationToken cancellationToken = default)
    {
        var emptyList = new PaginatedList<T>(new List<T>(), 0, page, limit);
        return Task.FromResult(emptyList);
    }

    public Task CreateAsync(T entity, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
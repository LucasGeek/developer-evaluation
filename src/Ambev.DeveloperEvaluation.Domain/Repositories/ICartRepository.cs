using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

public interface ICartRepository
{
    Task<Cart?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PaginatedList<Cart>> GetAllAsync(int page = 1, int limit = 20, string? sort = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<PaginatedList<Cart>> GetByUserIdRangeAsync(Guid startUserId, Guid endUserId, int page = 1, int limit = 20, string? sort = null, CancellationToken cancellationToken = default);
    Task<Cart> CreateAsync(Cart cart, CancellationToken cancellationToken = default);
    Task<Cart> UpdateAsync(Cart cart, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
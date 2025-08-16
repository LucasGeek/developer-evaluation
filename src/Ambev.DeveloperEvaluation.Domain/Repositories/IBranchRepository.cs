using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

/// <summary>
/// Repository interface for Branch entity operations
/// </summary>
public interface IBranchRepository
{
    /// <summary>
    /// Creates a new branch in the repository
    /// </summary>
    /// <param name="branch">The branch to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created branch</returns>
    Task<Branch> CreateAsync(Branch branch, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a branch by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the branch</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The branch if found, null otherwise</returns>
    Task<Branch?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all branches with pagination support
    /// </summary>
    /// <param name="page">Page number (starting from 1)</param>
    /// <param name="size">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of branches</returns>
    Task<PaginatedList<Branch>> GetAllAsync(int page = 1, int size = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves active branches with pagination support
    /// </summary>
    /// <param name="page">Page number (starting from 1)</param>
    /// <param name="size">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of active branches</returns>
    Task<PaginatedList<Branch>> GetActiveAsync(int page = 1, int size = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing branch in the repository
    /// </summary>
    /// <param name="branch">The branch to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated branch</returns>
    Task<Branch> UpdateAsync(Branch branch, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a branch from the repository
    /// </summary>
    /// <param name="id">The unique identifier of the branch to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the branch was deleted, false if not found</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a branch exists by name
    /// </summary>
    /// <param name="name">The branch name to check</param>
    /// <param name="excludeId">Optional ID to exclude from the check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
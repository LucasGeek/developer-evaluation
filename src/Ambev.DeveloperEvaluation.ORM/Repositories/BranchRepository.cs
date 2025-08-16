using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

/// <summary>
/// Implementation of IBranchRepository using Entity Framework Core
/// </summary>
public class BranchRepository : IBranchRepository
{
    private readonly DefaultContext _context;

    /// <summary>
    /// Initializes a new instance of BranchRepository
    /// </summary>
    /// <param name="context">The database context</param>
    public BranchRepository(DefaultContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Creates a new branch in the database
    /// </summary>
    /// <param name="branch">The branch to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created branch</returns>
    public async Task<Branch> CreateAsync(Branch branch, CancellationToken cancellationToken = default)
    {
        _context.Branches.Add(branch);
        await _context.SaveChangesAsync(cancellationToken);
        return branch;
    }

    /// <summary>
    /// Retrieves a branch by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the branch</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The branch if found, null otherwise</returns>
    public async Task<Branch?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Branches.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    /// <summary>
    /// Retrieves all branches with pagination support
    /// </summary>
    /// <param name="page">Page number (starting from 1)</param>
    /// <param name="size">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of branches</returns>
    public async Task<PaginatedList<Branch>> GetAllAsync(int page = 1, int size = 10, CancellationToken cancellationToken = default)
    {
        var query = _context.Branches.OrderBy(b => b.Name).AsQueryable();
        
        var totalCount = await query.CountAsync(cancellationToken);
        var branches = await query
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        return new PaginatedList<Branch>(branches, totalCount, page, size);
    }

    /// <summary>
    /// Retrieves active branches with pagination support
    /// </summary>
    /// <param name="page">Page number (starting from 1)</param>
    /// <param name="size">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of active branches</returns>
    public async Task<PaginatedList<Branch>> GetActiveAsync(int page = 1, int size = 10, CancellationToken cancellationToken = default)
    {
        var query = _context.Branches
            .Where(b => b.IsActive)
            .OrderBy(b => b.Name)
            .AsQueryable();
        
        var totalCount = await query.CountAsync(cancellationToken);
        var branches = await query
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        return new PaginatedList<Branch>(branches, totalCount, page, size);
    }

    /// <summary>
    /// Updates an existing branch in the database
    /// </summary>
    /// <param name="branch">The branch to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated branch</returns>
    public async Task<Branch> UpdateAsync(Branch branch, CancellationToken cancellationToken = default)
    {
        _context.Branches.Update(branch);
        await _context.SaveChangesAsync(cancellationToken);
        return branch;
    }

    /// <summary>
    /// Deletes a branch from the database
    /// </summary>
    /// <param name="id">The unique identifier of the branch to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the branch was deleted, false if not found</returns>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var branch = await GetByIdAsync(id, cancellationToken);
        if (branch == null)
            return false;

        _context.Branches.Remove(branch);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Checks if a branch exists by name
    /// </summary>
    /// <param name="name">The branch name to check</param>
    /// <param name="excludeId">Optional ID to exclude from the check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if exists, false otherwise</returns>
    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Branches.Where(b => b.Name == name);
        
        if (excludeId.HasValue)
            query = query.Where(b => b.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }
}
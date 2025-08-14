using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

/// <summary>
/// Implementation of IUserRepository using Entity Framework Core
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly DefaultContext _context;

    /// <summary>
    /// Initializes a new instance of UserRepository
    /// </summary>
    /// <param name="context">The database context</param>
    public UserRepository(DefaultContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Creates a new user in the database
    /// </summary>
    /// <param name="user">The user to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created user</returns>
    public async Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    /// <summary>
    /// Retrieves a user by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user if found, null otherwise</returns>
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FirstOrDefaultAsync(o=> o.Id == id, cancellationToken);
    }

    /// <summary>
    /// Retrieves a user by their email address
    /// </summary>
    /// <param name="email">The email address to search for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user if found, null otherwise</returns>
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    /// <summary>
    /// Deletes a user from the database
    /// </summary>
    /// <param name="id">The unique identifier of the user to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the user was deleted, false if not found</returns>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await GetByIdAsync(id, cancellationToken);
        if (user == null)
            return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Updates an existing user in the database
    /// </summary>
    /// <param name="user">The user to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated user</returns>
    public async Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        user.UpdatedAt = DateTime.UtcNow;
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    /// <summary>
    /// Retrieves all users with pagination, sorting and filtering
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="limit">Number of items per page</param>
    /// <param name="sort">Sort expression</param>
    /// <param name="email">Email filter</param>
    /// <param name="username">Username filter</param>
    /// <param name="role">Role filter</param>
    /// <param name="status">Status filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of users</returns>
    public async Task<PaginatedList<User>> GetAllAsync(int page = 1, int limit = 10, string? sort = null, 
        string? email = null, string? username = null, string? role = null, string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Users.AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(email))
            query = query.Where(u => u.Email.Contains(email));

        if (!string.IsNullOrEmpty(username))
            query = query.Where(u => u.Username.Contains(username));

        if (!string.IsNullOrEmpty(role))
        {
            if (Enum.TryParse<UserRole>(role, true, out var userRole))
                query = query.Where(u => u.Role == userRole);
        }

        if (!string.IsNullOrEmpty(status))
        {
            if (Enum.TryParse<UserStatus>(status, true, out var userStatus))
                query = query.Where(u => u.Status == userStatus);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting (predefined safe options)
        if (!string.IsNullOrEmpty(sort))
        {
            query = sort.ToLower() switch
            {
                "username asc" => query.OrderBy(u => u.Username),
                "username desc" => query.OrderByDescending(u => u.Username),
                "email asc" => query.OrderBy(u => u.Email),
                "email desc" => query.OrderByDescending(u => u.Email),
                "role asc" => query.OrderBy(u => u.Role),
                "role desc" => query.OrderByDescending(u => u.Role),
                "status asc" => query.OrderBy(u => u.Status),
                "status desc" => query.OrderByDescending(u => u.Status),
                "createdat asc" => query.OrderBy(u => u.CreatedAt),
                "createdat desc" => query.OrderByDescending(u => u.CreatedAt),
                _ => query.OrderBy(u => u.CreatedAt) // Default sort
            };
        }
        else
        {
            query = query.OrderBy(u => u.CreatedAt);
        }

        // Apply pagination
        var skip = (page - 1) * limit;
        var users = await query
            .Skip(skip)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return new PaginatedList<User>(users, totalCount, page, limit);
    }
}

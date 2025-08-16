using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class CartRepository : ICartRepository
{
    private readonly DefaultContext _context;

    public CartRepository(DefaultContext context)
    {
        _context = context;
    }

    public async Task<Cart?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Carts
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Carts
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
    }

    public async Task<PaginatedList<Cart>> GetAllAsync(int page = 1, int limit = 20, string? sort = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Carts.Include(c => c.Products).AsQueryable();

        // Apply date filtering
        if (startDate.HasValue)
            query = query.Where(c => c.Date >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(c => c.Date <= endDate.Value);

        // Apply sorting
        query = sort?.ToLower() switch
        {
            "date" => query.OrderBy(c => c.Date),
            "date-desc" => query.OrderByDescending(c => c.Date),
            "userid" => query.OrderBy(c => c.UserId),
            "userid-desc" => query.OrderByDescending(c => c.UserId),
            _ => query.OrderByDescending(c => c.Date)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return new PaginatedList<Cart>(items, totalCount, page, limit);
    }

    public async Task<PaginatedList<Cart>> GetByUserIdRangeAsync(Guid startUserId, Guid endUserId, int page = 1, int limit = 20, string? sort = null, CancellationToken cancellationToken = default)
    {
        // Filter by Guid range using CompareTo method for Guid comparison
        var query = _context.Carts
            .Include(c => c.Products)
            .Where(c => c.UserId.CompareTo(startUserId) >= 0 && c.UserId.CompareTo(endUserId) <= 0);

        // Apply sorting
        query = sort?.ToLower() switch
        {
            "date" => query.OrderBy(c => c.Date),
            "date-desc" => query.OrderByDescending(c => c.Date),
            "userid" => query.OrderBy(c => c.UserId),
            "userid-desc" => query.OrderByDescending(c => c.UserId),
            _ => query.OrderByDescending(c => c.Date)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return new PaginatedList<Cart>(items, totalCount, page, limit);
    }

    public async Task<Cart> CreateAsync(Cart cart, CancellationToken cancellationToken = default)
    {
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync(cancellationToken);
        return cart;
    }

    public async Task<Cart> UpdateAsync(Cart cart, CancellationToken cancellationToken = default)
    {
        _context.Carts.Update(cart);
        await _context.SaveChangesAsync(cancellationToken);
        return cart;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cart = await GetByIdAsync(id, cancellationToken);
        if (cart != null)
        {
            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
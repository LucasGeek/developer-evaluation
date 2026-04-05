using DeveloperEvaluation.Domain.Common;
using DeveloperEvaluation.Domain.Entities;
using DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DeveloperEvaluation.ORM.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly DefaultContext _context;

    public ProductRepository(DefaultContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<PaginatedList<Product>> GetAllAsync(int page = 1, int limit = 20, string? sort = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Products.AsQueryable();
        
        // Apply sorting
        query = sort?.ToLower() switch
        {
            "price" => query.OrderBy(p => p.Price),
            "price-desc" => query.OrderByDescending(p => p.Price),
            "title" => query.OrderBy(p => p.Title),
            "title-desc" => query.OrderByDescending(p => p.Title),
            _ => query.OrderBy(p => p.Id)
        };
        
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);
            
        return new PaginatedList<Product>(items, totalCount, page, limit);
    }

    public async Task<PaginatedList<Product>> GetByCategoryAsync(string category, int page = 1, int limit = 20, string? sort = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Products.Where(p => p.Category.ToLower() == category.ToLower());
        
        // Apply sorting
        query = sort?.ToLower() switch
        {
            "price" => query.OrderBy(p => p.Price),
            "price-desc" => query.OrderByDescending(p => p.Price),
            "title" => query.OrderBy(p => p.Title),
            "title-desc" => query.OrderByDescending(p => p.Title),
            _ => query.OrderBy(p => p.Id)
        };
        
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);
            
        return new PaginatedList<Product>(items, totalCount, page, limit);
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Select(p => p.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(cancellationToken);
    }

    public async Task<Product> CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);
        return product;
    }

    public async Task<Product> UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync(cancellationToken);
        return product;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await GetByIdAsync(id, cancellationToken);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
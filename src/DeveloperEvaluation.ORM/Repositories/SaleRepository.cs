using DeveloperEvaluation.Domain.Entities;
using DeveloperEvaluation.Domain.Repositories;
using DeveloperEvaluation.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace DeveloperEvaluation.ORM.Repositories;

public class SaleRepository : ISaleRepository
{
    private readonly DefaultContext _context;

    public SaleRepository(DefaultContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateNextSaleNumberAsync(Guid branchId)
    {
        var branchPrefix = branchId.ToString("N").Substring(0, 8).ToUpper();
        
        var lastSale = await _context.Sales
            .Where(s => s.BranchId == branchId)
            .OrderByDescending(s => s.SaleNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastSale != null && !string.IsNullOrEmpty(lastSale.SaleNumber))
        {
            var parts = lastSale.SaleNumber.Split('-');
            if (parts.Length == 2 && int.TryParse(parts[1], out var currentNumber))
            {
                nextNumber = currentNumber + 1;
            }
        }

        return $"BRANCH{branchPrefix}-{nextNumber:D4}";
    }

    public async Task CreateAsync(Sale sale)
    {
        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();
    }

    public async Task<Sale?> GetByIdAsync(Guid id)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Sale?> GetBySaleNumberAsync(string saleNumber, Guid branchId)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.SaleNumber == saleNumber && s.BranchId == branchId);
    }

    public async Task UpdateAsync(Sale sale)
    {
        var existingIds = (await _context.SaleItems
            .AsNoTracking()
            .Where(si => si.SaleId == sale.Id)
            .Select(si => si.Id)
            .ToListAsync()).ToHashSet();

        foreach (var item in sale.Items)
        {
            var entry = _context.Entry(item);
            if (!existingIds.Contains(item.Id) && entry.State != EntityState.Added && entry.State != EntityState.Deleted)
                entry.State = EntityState.Added;
        }
        await _context.SaveChangesAsync();
    }

    public async Task<(List<Sale> Sales, int TotalCount)> ListAsync(
        int page, 
        int size, 
        string? order = null, 
        DateTime? minDate = null, 
        DateTime? maxDate = null, 
        Guid? customerId = null, 
        Guid? branchId = null, 
        bool? cancelled = null,
        string? saleNumber = null)
    {
        var query = _context.Sales.Include(s => s.Items).AsQueryable();

        if (minDate.HasValue)
            query = query.Where(s => s.Date >= minDate.Value);

        if (maxDate.HasValue)
            query = query.Where(s => s.Date <= maxDate.Value);

        if (customerId.HasValue)
            query = query.Where(s => s.CustomerId == customerId.Value);

        if (branchId.HasValue)
            query = query.Where(s => s.BranchId == branchId.Value);

        if (cancelled.HasValue)
            query = query.Where(s => s.Cancelled == cancelled.Value);

        if (!string.IsNullOrEmpty(saleNumber))
            query = query.Where(s => s.SaleNumber.Contains(saleNumber));

        if (!string.IsNullOrEmpty(order))
        {
            query = order.ToLower() switch
            {
                "date" => query.OrderBy(s => s.Date),
                "date_desc" => query.OrderByDescending(s => s.Date),
                "total" => query.OrderBy(s => s.TotalAmount),
                "total_desc" => query.OrderByDescending(s => s.TotalAmount),
                "salenumber" => query.OrderBy(s => s.SaleNumber),
                "salenumber_desc" => query.OrderByDescending(s => s.SaleNumber),
                _ => query.OrderByDescending(s => s.CreatedAt)
            };
        }
        else
        {
            query = query.OrderByDescending(s => s.CreatedAt);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return (items, totalCount);
    }
}
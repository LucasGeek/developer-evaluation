using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

public interface ISaleRepository
{
    Task<string> GenerateNextSaleNumberAsync(Guid branchId);
    Task CreateAsync(Sale sale);
    Task<Sale?> GetByIdAsync(Guid id);
    Task<Sale?> GetBySaleNumberAsync(string saleNumber, Guid branchId);
    Task UpdateAsync(Sale sale);
    Task<(List<Sale> Sales, int TotalCount)> ListAsync(
        int page, 
        int size, 
        string? order = null, 
        DateTime? minDate = null, 
        DateTime? maxDate = null, 
        Guid? customerId = null, 
        Guid? branchId = null, 
        bool? cancelled = null,
        string? saleNumber = null);
}
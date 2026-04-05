namespace DeveloperEvaluation.WebApi.Features.Sales.ListSales;

public class ListSalesResponse
{
    public List<ListSaleItemResponse> Sales { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}

public class ListSaleItemResponse
{
    public Guid Id { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerDescription { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public Guid BranchId { get; set; }
    public string BranchDescription { get; set; } = string.Empty;
    public bool Cancelled { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int ItemCount { get; set; }
}
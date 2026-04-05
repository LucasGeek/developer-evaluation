namespace DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;

public class UpdateSaleResponse
{
    public Guid Id { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerDescription { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public Guid BranchId { get; set; }
    public string BranchDescription { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
    public List<UpdateSaleItemResponse> Items { get; set; } = new();
}

public class UpdateSaleItemResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductDescription { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
}
namespace DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;

public class UpdateSaleRequest
{
    public string CustomerDescription { get; set; }
    public string BranchDescription { get; set; }
    public DateTime Date { get; set; }
    public List<UpdateSaleItemRequest> Items { get; set; } = new();
}

public class UpdateSaleItemRequest
{
    public string ProductDescription { get; set; }
    public decimal UnitPrice { get; set; }
    public Guid? Id { get; set; } // null for new items
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
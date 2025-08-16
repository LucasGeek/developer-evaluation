namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;

public class UpdateSaleRequest
{
    public DateTime Date { get; set; }
    public List<UpdateSaleItemRequest> Items { get; set; } = new();
}

public class UpdateSaleItemRequest
{
    public Guid? Id { get; set; } // null for new items
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
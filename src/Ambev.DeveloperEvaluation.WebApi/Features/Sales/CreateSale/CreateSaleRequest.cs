namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;

/// <summary>
/// Request for creating a new sale
/// </summary>
public class CreateSaleRequest
{
    public string BranchDescription { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerDescription { get; set; }
    public DateTime Date { get; set; }
    /// <summary>
    /// The ID of the branch where the sale is made
    /// </summary>
    public Guid BranchId { get; set; }
    
    /// <summary>
    /// The items being purchased
    /// Customer ID is automatically extracted from the JWT token
    /// </summary>
    public List<CreateSaleItemRequest> Items { get; set; } = new();
}

/// <summary>
/// Request for a sale item
/// </summary>
public class CreateSaleItemRequest
{
    public string ProductDescription { get; set; }
    public decimal UnitPrice { get; set; }
    /// <summary>
    /// The ID of the product being purchased
    /// </summary>
    public Guid ProductId { get; set; }
    
    /// <summary>
    /// The quantity of the product being purchased
    /// Must be between 1 and 20 (business rule)
    /// </summary>
    public int Quantity { get; set; }
}
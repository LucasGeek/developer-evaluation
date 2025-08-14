namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

public class CancelSaleItemResult
{
    public Guid SaleId { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public Guid ItemId { get; set; }
    public string ProductDescription { get; set; } = string.Empty;
    public bool ItemRemoved { get; set; }
    public decimal NewTotalAmount { get; set; }
    public string Message { get; set; } = string.Empty;
}
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public class UpdateSaleCommand : IRequest<UpdateSaleResult>
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public string CustomerDescription { get; set; } = string.Empty;
    public string BranchDescription { get; set; } = string.Empty;
    public List<UpdateSaleItemCommand> Items { get; set; } = new();
}

public class UpdateSaleItemCommand
{
    public Guid? Id { get; set; } // null for new items
    public Guid ProductId { get; set; }
    public string ProductDescription { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
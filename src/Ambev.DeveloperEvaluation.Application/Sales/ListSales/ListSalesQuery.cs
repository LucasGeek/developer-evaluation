using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

public class ListSalesQuery : IRequest<ListSalesResult>
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public string? Order { get; set; }
    public DateTime? MinDate { get; set; }
    public DateTime? MaxDate { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? BranchId { get; set; }
    public bool? Cancelled { get; set; }
    public string? SaleNumber { get; set; }

    public ListSalesQuery(int page = 1, int size = 10, string? order = null)
    {
        Page = page > 0 ? page : 1;
        Size = size > 0 ? (size <= 100 ? size : 100) : 10;
        Order = order;
    }
}
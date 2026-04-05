using DeveloperEvaluation.Application.Carts.GetCart;

namespace DeveloperEvaluation.Application.Carts.ListCarts;

public class ListCartsResult
{
    public List<GetCartResult> Carts { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
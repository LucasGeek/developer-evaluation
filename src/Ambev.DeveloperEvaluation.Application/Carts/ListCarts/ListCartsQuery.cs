using Ambev.DeveloperEvaluation.ORM.CQRS;

namespace Ambev.DeveloperEvaluation.Application.Carts.ListCarts;

public class ListCartsQuery : IQuery<ListCartsResult>
{
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 20;
    public string? Sort { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? StartUserId { get; set; }
    public int? EndUserId { get; set; }
}
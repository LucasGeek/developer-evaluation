using DeveloperEvaluation.ORM.CQRS;

namespace DeveloperEvaluation.Application.Products.ListProducts;

public class ListProductsQuery : IQuery<ListProductsResult>
{
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 20;
    public string? Sort { get; set; }
    public string? Category { get; set; }
}
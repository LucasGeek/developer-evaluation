using Ambev.DeveloperEvaluation.ORM.CQRS;

namespace Ambev.DeveloperEvaluation.Application.Products.CreateProduct;

public class CreateProductCommand : ICommand<Guid>
{
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public RatingCommand Rating { get; set; } = new();
}

public class RatingCommand
{
    public double Rate { get; set; }
    public int Count { get; set; }
}
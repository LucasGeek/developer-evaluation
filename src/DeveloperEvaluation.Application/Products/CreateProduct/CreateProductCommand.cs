using DeveloperEvaluation.ORM.CQRS;

namespace DeveloperEvaluation.Application.Products.CreateProduct;

public class CreateProductCommand : ICommand<Guid>
{
    public CreateProductCommand() {}
    public CreateProductCommand(string title, decimal price, string description, string category, string image)
    {
        Title = title;
        Price = price;
        Description = description;
        Category = category;
        Image = image;
    }
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
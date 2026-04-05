namespace DeveloperEvaluation.WebApi.Features.Products;

public class CreateProductRequest
{
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public ProductRatingRequest Rating { get; set; } = new();
}

public class ProductRatingRequest
{
    public decimal Rate { get; set; }
    public int Count { get; set; }
}
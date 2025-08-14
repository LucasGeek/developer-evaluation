using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class Product : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public string Image { get; private set; } = string.Empty;
    public Rating Rating { get; private set; } = new();

    protected Product() { }

    public Product(string title, decimal price, string description, string category, string image)
    {
        Title = title;
        Price = price;
        Description = description;
        Category = category;
        Image = image;
        Rating = new Rating();
    }

    public void UpdateDetails(string title, decimal price, string description, string category, string image)
    {
        Title = title;
        Price = price;
        Description = description;
        Category = category;
        Image = image;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateRating(double rate, int count)
    {
        Rating = new Rating { Rate = rate, Count = count };
        UpdatedAt = DateTime.UtcNow;
    }
}

public class Rating
{
    public double Rate { get; set; }
    public int Count { get; set; }
}
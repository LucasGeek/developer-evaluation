using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DeveloperEvaluation.ORM.MongoDB.ReadModels;

public class ProductReadModel
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;
    
    public decimal Price { get; set; }
    
    public string Description { get; set; } = string.Empty;
    
    public string Category { get; set; } = string.Empty;
    
    public string Image { get; set; } = string.Empty;
    
    public ProductRatingReadModel Rating { get; set; } = new();
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
}

public class ProductRatingReadModel
{
    public double Rate { get; set; }
    
    public int Count { get; set; }
}
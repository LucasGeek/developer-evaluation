using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Ambev.DeveloperEvaluation.ORM.MongoDB.ReadModels;

public class SaleReadModel
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    public string SaleNumber { get; set; } = string.Empty;
    
    public Guid BranchId { get; set; }
    
    public string CustomerDescription { get; set; } = string.Empty;
    
    public DateTime Date { get; set; }
    
    public decimal TotalAmount { get; set; }
    
    public bool IsCancelled { get; set; }
    
    public DateTime? CancelledAt { get; set; }
    
    public List<SaleItemReadModel> Items { get; set; } = new();
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
}

public class SaleItemReadModel
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }
    
    public string ProductDescription { get; set; } = string.Empty;
    
    public int Quantity { get; set; }
    
    public decimal UnitPrice { get; set; }
    
    public decimal Discount { get; set; }
    
    public decimal TotalAmount { get; set; }
}
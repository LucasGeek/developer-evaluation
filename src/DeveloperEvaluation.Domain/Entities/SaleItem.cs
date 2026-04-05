using DeveloperEvaluation.Domain.Common;

namespace DeveloperEvaluation.Domain.Entities;

public class SaleItem : BaseEntity
{
    public Guid SaleId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductDescription { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Discount { get; private set; }
    public decimal Total { get; private set; }

    public SaleItem()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public SaleItem(Guid saleId, Guid productId, string productDescription, int quantity, decimal unitPrice) : this()
    {
        SaleId = saleId;
        ProductId = productId;
        ProductDescription = productDescription;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public void UpdateQuantityAndPrice(int quantity, decimal unitPrice)
    {
        if (quantity > 20)
            throw new InvalidOperationException("Quantity cannot exceed 20 per item.");
        
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public void ApplyDiscount()
    {
        if (Quantity > 20)
            throw new InvalidOperationException("Quantity cannot exceed 20 per item.");

        // Business rules: 
        // - No discount for quantities below 4
        // - 10% discount for 4-9 items
        // - 20% discount for 10-20 items
        // - No sales above 20 items allowed
        Discount = Quantity switch
        {
            >= 4 and < 10 => Math.Round(UnitPrice * Quantity * 0.10m, 2),
            >= 10 and <= 20 => Math.Round(UnitPrice * Quantity * 0.20m, 2),
            < 4 => 0, // Explicitly no discount for less than 4 items
            _ => throw new InvalidOperationException("Invalid quantity for discount calculation.")
        };

        Total = Math.Round((UnitPrice * Quantity) - Discount, 2);
    }
}
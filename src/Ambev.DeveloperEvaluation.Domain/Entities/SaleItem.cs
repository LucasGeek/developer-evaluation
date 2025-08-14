using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

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

        Discount = Quantity switch
        {
            >= 4 and < 10 => Math.Round(UnitPrice * Quantity * 0.10m, 2),
            >= 10 and <= 20 => Math.Round(UnitPrice * Quantity * 0.20m, 2),
            _ => 0
        };

        Total = Math.Round((UnitPrice * Quantity) - Discount, 2);
    }
}
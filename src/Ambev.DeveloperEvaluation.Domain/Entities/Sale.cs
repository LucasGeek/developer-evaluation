using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class Sale : BaseEntity
{
    public string SaleNumber { get; private set; } = string.Empty;
    public DateTime Date { get; private set; }
    public Guid CustomerId { get; private set; }
    public string CustomerDescription { get; private set; } = string.Empty;
    public decimal TotalAmount { get; private set; }
    public Guid BranchId { get; private set; }
    public string BranchDescription { get; private set; } = string.Empty;
    public bool Cancelled { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();
    public List<SaleItem> Items { get; private set; } = new();

    public Sale()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Sale(string saleNumber, DateTime date, Guid customerId, string customerDescription, Guid branchId, string branchDescription) : this()
    {
        SaleNumber = saleNumber;
        Date = date;
        CustomerId = customerId;
        CustomerDescription = customerDescription;
        BranchId = branchId;
        BranchDescription = branchDescription;
    }

    public void AddItem(SaleItem item)
    {
        if (Items.Count(i => i.ProductId == item.ProductId) >= 20)
            throw new InvalidOperationException("Cannot exceed 20 items of same product.");
        
        item.ApplyDiscount();
        Items.Add(item);
        RecalculateTotal();
    }

    public void UpdateItem(Guid productId, int quantity, decimal unitPrice)
    {
        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            item.UpdateQuantityAndPrice(quantity, unitPrice);
            item.ApplyDiscount();
            RecalculateTotal();
        }
    }

    public void RemoveItem(Guid productId)
    {
        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            Items.Remove(item);
            RecalculateTotal();
        }
    }

    public void RemoveItemById(Guid itemId)
    {
        var item = Items.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            Items.Remove(item);
            RecalculateTotal();
        }
    }

    public void Cancel()
    {
        Cancelled = true;
        CancelledAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecalculateTotal()
    {
        TotalAmount = Math.Round(Items.Sum(i => i.Total), 2);
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetSaleNumber(string saleNumber)
    {
        SaleNumber = saleNumber;
    }

    public void UpdateSaleDetails(DateTime date, string customerDescription, string branchDescription)
    {
        Date = date;
        CustomerDescription = customerDescription;
        BranchDescription = branchDescription;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearItems()
    {
        Items.Clear();
        UpdatedAt = DateTime.UtcNow;
    }
}
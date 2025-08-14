using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class Cart : BaseEntity
{
    public Guid UserId { get; private set; }
    public DateTime Date { get; private set; }
    public List<CartItem> Products { get; private set; } = new();

    protected Cart() { }

    public Cart(Guid userId)
    {
        UserId = userId;
        Date = DateTime.UtcNow;
        Products = new List<CartItem>();
    }

    public void AddProduct(Guid productId, int quantity)
    {
        var existingItem = Products.FirstOrDefault(p => p.ProductId == productId);
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            Products.Add(new CartItem(productId, quantity));
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateProductQuantity(Guid productId, int quantity)
    {
        var item = Products.FirstOrDefault(p => p.ProductId == productId);
        if (item != null)
        {
            if (quantity <= 0)
            {
                Products.Remove(item);
            }
            else
            {
                item.UpdateQuantity(quantity);
            }
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void RemoveProduct(Guid productId)
    {
        var item = Products.FirstOrDefault(p => p.ProductId == productId);
        if (item != null)
        {
            Products.Remove(item);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void Clear()
    {
        Products.Clear();
        UpdatedAt = DateTime.UtcNow;
    }
}

public class CartItem
{
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }

    protected CartItem() { }

    public CartItem(Guid productId, int quantity)
    {
        ProductId = productId;
        Quantity = quantity;
    }

    public void UpdateQuantity(int quantity)
    {
        Quantity = quantity;
    }
}
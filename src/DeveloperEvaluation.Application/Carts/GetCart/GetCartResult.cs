namespace DeveloperEvaluation.Application.Carts.GetCart;

public class GetCartResult
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime Date { get; set; }
    public List<CartItemResult> Products { get; set; } = new();
}

public class CartItemResult
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
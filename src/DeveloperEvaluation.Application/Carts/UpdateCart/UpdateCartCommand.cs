using DeveloperEvaluation.ORM.CQRS;

namespace DeveloperEvaluation.Application.Carts.UpdateCart;

public class UpdateCartCommand : ICommand<bool>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public List<CartItemRequest> Products { get; set; } = new();
}

public class CartItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
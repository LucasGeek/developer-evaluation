using Ambev.DeveloperEvaluation.ORM.CQRS;

namespace Ambev.DeveloperEvaluation.Application.Carts.CreateCart;

public class CreateCartCommand : ICommand<Guid>
{
    public Guid UserId { get; set; }
    public List<CartItemRequest> Products { get; set; } = new();
}

public class CartItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
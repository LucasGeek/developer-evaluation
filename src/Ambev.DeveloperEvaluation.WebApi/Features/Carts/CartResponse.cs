namespace Ambev.DeveloperEvaluation.WebApi.Features.Carts;

public class CartResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime Date { get; set; }
    public List<CartItemResponse> Products { get; set; } = new();
}

public class CartItemResponse
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public class CartListResponse
{
    public List<CartResponse> Data { get; set; } = new();
    public int TotalItems { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}

public class CreateCartRequest
{
    public Guid UserId { get; set; }
    public DateTime Date { get; set; }
    public List<CartItemRequest> Products { get; set; } = new();
}

public class CartItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public class CreateCartResponse
{
    public Guid Id { get; set; }
}
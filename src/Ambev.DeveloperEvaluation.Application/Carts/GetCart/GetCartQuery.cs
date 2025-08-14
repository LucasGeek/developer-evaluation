using Ambev.DeveloperEvaluation.ORM.CQRS;

namespace Ambev.DeveloperEvaluation.Application.Carts.GetCart;

public class GetCartQuery : IQuery<GetCartResult?>
{
    public Guid Id { get; set; }

    public GetCartQuery(Guid id)
    {
        Id = id;
    }
}
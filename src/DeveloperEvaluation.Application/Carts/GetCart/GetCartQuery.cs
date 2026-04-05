using DeveloperEvaluation.ORM.CQRS;

namespace DeveloperEvaluation.Application.Carts.GetCart;

public class GetCartQuery : IQuery<GetCartResult?>
{
    public Guid Id { get; set; }

    public GetCartQuery(Guid id)
    {
        Id = id;
    }
}
using DeveloperEvaluation.ORM.CQRS;

namespace DeveloperEvaluation.Application.Products.GetProduct;

public class GetProductQuery : IQuery<GetProductResult?>
{
    public Guid Id { get; set; }
    
    public GetProductQuery(Guid id)
    {
        Id = id;
    }
}
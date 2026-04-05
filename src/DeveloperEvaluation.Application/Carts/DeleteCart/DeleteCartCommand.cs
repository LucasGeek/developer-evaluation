using DeveloperEvaluation.ORM.CQRS;

namespace DeveloperEvaluation.Application.Carts.DeleteCart;

public class DeleteCartCommand : ICommand<bool>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public DeleteCartCommand(Guid id, Guid userId)
    {
        Id = id;
        UserId = userId;
    }
}
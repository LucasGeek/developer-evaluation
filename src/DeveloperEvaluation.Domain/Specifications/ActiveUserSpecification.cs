using DeveloperEvaluation.Domain.Entities;
using DeveloperEvaluation.Domain.Enums;

namespace DeveloperEvaluation.Domain.Specifications;

public class ActiveUserSpecification : ISpecification<User>
{
    public bool IsSatisfiedBy(User user)
    {
        return user.Status == UserStatus.Active;
    }
}

using DeveloperEvaluation.ORM.CQRS;

namespace DeveloperEvaluation.Application.Users.UpdateUser;

public class UpdateUserCommand : ICommand<UpdateUserResult>
{
    public Guid Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string Role { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}
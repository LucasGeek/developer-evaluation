using DeveloperEvaluation.ORM.CQRS;

namespace DeveloperEvaluation.Application.Users.ListUsers;

public class ListUsersQuery : IQuery<ListUsersResult>
{
    public int Page { get; init; } = 1;
    public int Limit { get; init; } = 10;
    public string? Sort { get; init; }
    public string? Email { get; init; }
    public string? Username { get; init; }
    public string? Role { get; init; }
    public string? Status { get; init; }
}
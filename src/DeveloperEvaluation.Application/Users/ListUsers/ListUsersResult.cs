using DeveloperEvaluation.Application.Users.GetUser;

namespace DeveloperEvaluation.Application.Users.ListUsers;

public class ListUsersResult
{
    public List<GetUserResult> Users { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
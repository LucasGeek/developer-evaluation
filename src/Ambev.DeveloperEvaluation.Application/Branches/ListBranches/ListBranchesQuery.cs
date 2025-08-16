using Ambev.DeveloperEvaluation.Application.Branches.GetBranch;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Branches.ListBranches;

/// <summary>
/// Query for listing branches with pagination
/// </summary>
public class ListBranchesQuery : IRequest<ListBranchesResult>
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public bool ActiveOnly { get; set; } = false;

    public ListBranchesQuery() { }

    public ListBranchesQuery(int page = 1, int size = 10, bool activeOnly = false)
    {
        Page = page > 0 ? page : 1;
        Size = size > 0 ? (size <= 100 ? size : 100) : 10;
        ActiveOnly = activeOnly;
    }
}

/// <summary>
/// Result for ListBranchesQuery
/// </summary>
public class ListBranchesResult
{
    public List<GetBranchResult> Branches { get; set; } = new();
    public int TotalCount { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}
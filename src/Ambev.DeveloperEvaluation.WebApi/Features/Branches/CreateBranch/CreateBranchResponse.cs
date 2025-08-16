namespace Ambev.DeveloperEvaluation.WebApi.Features.Branches.CreateBranch;

/// <summary>
/// Response for creating a new branch
/// </summary>
public class CreateBranchResponse
{
    /// <summary>
    /// The ID of the created branch
    /// </summary>
    public Guid Id { get; set; }
}
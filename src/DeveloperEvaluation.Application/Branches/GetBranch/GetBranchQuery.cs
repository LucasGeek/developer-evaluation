using MediatR;

namespace DeveloperEvaluation.Application.Branches.GetBranch;

/// <summary>
/// Query for retrieving a branch by ID
/// </summary>
public record GetBranchQuery(Guid Id) : IRequest<GetBranchResult?>;

/// <summary>
/// Result for GetBranchQuery
/// </summary>
public class GetBranchResult
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
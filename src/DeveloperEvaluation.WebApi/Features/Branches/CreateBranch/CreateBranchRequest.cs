namespace DeveloperEvaluation.WebApi.Features.Branches.CreateBranch;

/// <summary>
/// Request for creating a new branch
/// </summary>
public class CreateBranchRequest
{
    /// <summary>
    /// The branch name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The branch description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The branch address
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// The branch city
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// The branch state
    /// </summary>
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// The branch postal code
    /// </summary>
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>
    /// The branch phone number
    /// </summary>
    public string Phone { get; set; } = string.Empty;
}
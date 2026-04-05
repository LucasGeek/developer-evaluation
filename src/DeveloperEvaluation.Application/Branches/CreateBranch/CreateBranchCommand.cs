using MediatR;

namespace DeveloperEvaluation.Application.Branches.CreateBranch;

/// <summary>
/// Command for creating a new branch
/// </summary>
public record CreateBranchCommand(
    string Name,
    string Description,
    string Address,
    string City,
    string State,
    string PostalCode,
    string Phone
) : IRequest<Guid>;
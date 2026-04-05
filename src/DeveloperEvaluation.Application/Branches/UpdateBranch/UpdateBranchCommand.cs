using MediatR;

namespace DeveloperEvaluation.Application.Branches.UpdateBranch;

public record UpdateBranchCommand(
    Guid Id,
    string Name,
    string Description,
    string Address,
    string City,
    string State,
    string PostalCode,
    string Phone,
    bool IsActive
) : IRequest<bool>;

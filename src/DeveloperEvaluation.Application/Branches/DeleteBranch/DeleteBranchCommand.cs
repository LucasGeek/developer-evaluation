using MediatR;

namespace DeveloperEvaluation.Application.Branches.DeleteBranch;

public record DeleteBranchCommand(Guid Id) : IRequest<bool>;

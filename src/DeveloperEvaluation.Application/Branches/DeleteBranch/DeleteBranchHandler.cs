using DeveloperEvaluation.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DeveloperEvaluation.Application.Branches.DeleteBranch;

public class DeleteBranchHandler : IRequestHandler<DeleteBranchCommand, bool>
{
    private readonly IBranchRepository _branchRepository;
    private readonly ILogger<DeleteBranchHandler> _logger;

    public DeleteBranchHandler(IBranchRepository branchRepository, ILogger<DeleteBranchHandler> logger)
    {
        _branchRepository = branchRepository;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteBranchCommand request, CancellationToken cancellationToken)
    {
        var deleted = await _branchRepository.DeleteAsync(request.Id, cancellationToken);
        if (!deleted)
            _logger.LogWarning("Branch {BranchId} not found for deletion", request.Id);
        else
            _logger.LogInformation("Branch {BranchId} deleted", request.Id);
        return deleted;
    }
}

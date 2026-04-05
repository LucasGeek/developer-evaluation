using DeveloperEvaluation.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DeveloperEvaluation.Application.Branches.UpdateBranch;

public class UpdateBranchHandler : IRequestHandler<UpdateBranchCommand, bool>
{
    private readonly IBranchRepository _branchRepository;
    private readonly ILogger<UpdateBranchHandler> _logger;

    public UpdateBranchHandler(IBranchRepository branchRepository, ILogger<UpdateBranchHandler> logger)
    {
        _branchRepository = branchRepository;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateBranchCommand request, CancellationToken cancellationToken)
    {
        var branch = await _branchRepository.GetByIdAsync(request.Id, cancellationToken);
        if (branch == null)
        {
            _logger.LogWarning("Branch {BranchId} not found", request.Id);
            return false;
        }

        branch.UpdateDetails(request.Name, request.Description, request.Address, request.City, request.State, request.PostalCode, request.Phone);

        if (request.IsActive) branch.Activate();
        else branch.Deactivate();

        await _branchRepository.UpdateAsync(branch, cancellationToken);
        _logger.LogInformation("Branch {BranchId} updated", request.Id);
        return true;
    }
}

using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Branches.GetBranch;

/// <summary>
/// Handler for retrieving a branch by ID
/// </summary>
public class GetBranchHandler : IRequestHandler<GetBranchQuery, GetBranchResult?>
{
    private readonly IBranchRepository _branchRepository;
    private readonly IMapper _mapper;

    public GetBranchHandler(IBranchRepository branchRepository, IMapper mapper)
    {
        _branchRepository = branchRepository;
        _mapper = mapper;
    }

    public async Task<GetBranchResult?> Handle(GetBranchQuery request, CancellationToken cancellationToken)
    {
        var branch = await _branchRepository.GetByIdAsync(request.Id, cancellationToken);
        return branch == null ? null : _mapper.Map<GetBranchResult>(branch);
    }
}
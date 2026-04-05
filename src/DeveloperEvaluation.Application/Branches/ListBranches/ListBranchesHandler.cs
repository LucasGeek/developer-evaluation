using DeveloperEvaluation.Application.Branches.GetBranch;
using DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace DeveloperEvaluation.Application.Branches.ListBranches;

/// <summary>
/// Handler for listing branches with pagination
/// </summary>
public class ListBranchesHandler : IRequestHandler<ListBranchesQuery, ListBranchesResult>
{
    private readonly IBranchRepository _branchRepository;
    private readonly IMapper _mapper;

    public ListBranchesHandler(IBranchRepository branchRepository, IMapper mapper)
    {
        _branchRepository = branchRepository;
        _mapper = mapper;
    }

    public async Task<ListBranchesResult> Handle(ListBranchesQuery request, CancellationToken cancellationToken)
    {
        var paginatedBranches = request.ActiveOnly
            ? await _branchRepository.GetActiveAsync(request.Page, request.Size, cancellationToken)
            : await _branchRepository.GetAllAsync(request.Page, request.Size, cancellationToken);

        return new ListBranchesResult
        {
            Branches = _mapper.Map<List<GetBranchResult>>(paginatedBranches),
            TotalCount = paginatedBranches.TotalCount,
            CurrentPage = paginatedBranches.Page,
            TotalPages = paginatedBranches.TotalPages
        };
    }
}
using DeveloperEvaluation.Application.Branches.GetBranch;
using DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace DeveloperEvaluation.WebApi.Features.Branches.GetBranch;

/// <summary>
/// AutoMapper profile for GetBranch feature
/// </summary>
public class GetBranchProfile : Profile
{
    public GetBranchProfile()
    {
        CreateMap<Branch, GetBranchResult>();
        CreateMap<GetBranchResult, GetBranchResponse>();
    }
}
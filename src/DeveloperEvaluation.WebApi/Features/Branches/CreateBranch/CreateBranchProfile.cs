using DeveloperEvaluation.Application.Branches.CreateBranch;
using AutoMapper;

namespace DeveloperEvaluation.WebApi.Features.Branches.CreateBranch;

/// <summary>
/// AutoMapper profile for CreateBranch feature
/// </summary>
public class CreateBranchProfile : Profile
{
    public CreateBranchProfile()
    {
        CreateMap<CreateBranchRequest, CreateBranchCommand>();
    }
}